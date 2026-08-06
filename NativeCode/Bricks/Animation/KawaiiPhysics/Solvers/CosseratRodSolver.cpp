#include "CosseratRodSolver.h"
#include <cmath>

NS_BEGIN

namespace KawaiiPhysics
{
	namespace CosseratRodSolver
	{
		void InitializeRodElements(FCosseratRodData& Rod)
		{
			Rod.Elements.clear();
			if (Rod.Particles.size() < 2) return;

			for (size_t i = 0; i < Rod.Particles.size() - 1; ++i)
			{
				FCosseratRodElement elem;
				elem.RestPosition = Rod.Particles[i].RestPosition;
				elem.RestLength = Vec3Distance(Rod.Particles[i].RestPosition, Rod.Particles[i + 1].RestPosition);
				elem.bIsRoot = (i == 0);

				// Compute initial orientation from edge direction
				v3dxVector3 edgeDir = Vec3SafeNormal(Rod.Particles[i + 1].RestPosition - Rod.Particles[i].RestPosition);
				v3dxVector3 refUp(0, 1, 0);
				if (fabsf(Vec3Dot(edgeDir, refUp)) > 0.99f)
					refUp = v3dxVector3(1, 0, 0);

				v3dxVector3 right = Vec3SafeNormal(Vec3Cross(refUp, edgeDir));
				v3dxVector3 up = Vec3Cross(edgeDir, right);

				// Build quaternion from basis vectors
				float trace = right.X + up.Y + edgeDir.Z;
				if (trace > 0.0f)
				{
					float s = 0.5f / sqrtf(trace + 1.0f);
					elem.RestOrientation = v3dxQuaternion(
						(up.Z - edgeDir.Y) * s,
						(edgeDir.X - right.Z) * s,
						(right.Y - up.X) * s,
						0.25f / s);
				}
				else if (right.X > up.Y && right.X > edgeDir.Z)
				{
					float s = 2.0f * sqrtf(1.0f + right.X - up.Y - edgeDir.Z);
					elem.RestOrientation = v3dxQuaternion(
						0.25f * s,
						(right.Y + up.X) / s,
						(edgeDir.X + right.Z) / s,
						(up.Z - edgeDir.Y) / s);
				}
				else if (up.Y > edgeDir.Z)
				{
					float s = 2.0f * sqrtf(1.0f + up.Y - right.X - edgeDir.Z);
					elem.RestOrientation = v3dxQuaternion(
						(right.Y + up.X) / s,
						0.25f * s,
						(up.Z + edgeDir.Y) / s,
						(edgeDir.X - right.Z) / s);
				}
				else
				{
					float s = 2.0f * sqrtf(1.0f + edgeDir.Z - right.X - up.Y);
					elem.RestOrientation = v3dxQuaternion(
						(edgeDir.X + right.Z) / s,
						(up.Z + edgeDir.Y) / s,
						0.25f * s,
						(right.Y - up.X) / s);
				}

				elem.Orientation = elem.RestOrientation;
				elem.PrevOrientation = elem.RestOrientation;

				Rod.Elements.push_back(elem);
			}

			// Compute rest Darboux vectors
			for (size_t i = 0; i + 1 < Rod.Elements.size(); ++i)
			{
				Rod.Elements[i].RestDarboux = ComputeDarbouxVector(
					Rod.Elements[i].RestOrientation,
					Rod.Elements[i + 1].RestOrientation,
					Rod.Elements[i].RestLength);
			}
		}

		v3dxVector3 ComputeDarbouxVector(const v3dxQuaternion& Q1, const v3dxQuaternion& Q2, float SegmentLength)
		{
			v3dxQuaternion relQ = QuatMultiply(QuatInverse(Q1), Q2);

			// Ensure shortest path
			if (relQ.W < 0.0f)
			{
				relQ.X = -relQ.X;
				relQ.Y = -relQ.Y;
				relQ.Z = -relQ.Z;
				relQ.W = -relQ.W;
			}

			float invLen = (SegmentLength > KAWAII_SMALL_NUMBER) ? (2.0f / SegmentLength) : 0.0f;
			return v3dxVector3(relQ.X * invLen, relQ.Y * invLen, relQ.Z * invLen);
		}

		void SolveStretchShearConstraint(
			FCosseratRodData& Rod,
			int32_t ElementIndex,
			float dt)
		{
			if (ElementIndex < 0 || ElementIndex >= (int32_t)Rod.Elements.size()) return;
			if (ElementIndex + 1 >= (int32_t)Rod.Particles.size()) return;

			FSimParticle& P0 = Rod.Particles[ElementIndex];
			FSimParticle& P1 = Rod.Particles[ElementIndex + 1];
			FCosseratRodElement& Elem = Rod.Elements[ElementIndex];

			float w0 = (P0.PinMode == KPM_Dynamic) ? P0.InverseMass : 0.0f;
			float w1 = (P1.PinMode == KPM_Dynamic) ? P1.InverseMass : 0.0f;
			float wSum = w0 + w1;
			if (wSum < KAWAII_SMALL_NUMBER) return;

			v3dxVector3 edge = P1.Position - P0.Position;
			float edgeLen = Vec3Length(edge);
			if (edgeLen < KAWAII_SMALL_NUMBER) return;

			// Stretch constraint: maintain rest length
			v3dxVector3 edgeDir = edge * (1.0f / edgeLen);
			v3dxVector3 restEdge = QuatRotateVector(Elem.Orientation, v3dxVector3(0, 0, 1)) * Elem.RestLength;
			v3dxVector3 stretchError = edge - restEdge;

			float ssRate = (Rod.Elements.size() > 1) ? (float)ElementIndex / (float)(Rod.Elements.size() - 1) : 0.0f;
			float ssStiff = KawaiiClamp(Rod.StretchAndShearStiffness * Rod.StretchAndShearStiffnessCurve.Evaluate(ssRate), 0.0f, 1.0f);
			float compliance = (1.0f - ssStiff) / (dt * dt + KAWAII_SMALL_NUMBER);
			v3dxVector3 correction = stretchError * (-1.0f / (wSum + compliance + KAWAII_SMALL_NUMBER));

			P0.Position = P0.Position - correction * w0;
			P1.Position = P1.Position + correction * w1;
		}

		void SolveBendTwistConstraint(
			FCosseratRodData& Rod,
			int32_t ElementIndex,
			float dt)
		{
			if (ElementIndex + 1 >= (int32_t)Rod.Elements.size()) return;

			FCosseratRodElement& E0 = Rod.Elements[ElementIndex];
			FCosseratRodElement& E1 = Rod.Elements[ElementIndex + 1];

			v3dxVector3 darboux = ComputeDarbouxVector(E0.Orientation, E1.Orientation, E0.RestLength);
			v3dxVector3 darbouxError = darboux - E0.RestDarboux;

			if (Vec3IsNearlyZero(darbouxError)) return;

			float btRate = (Rod.Elements.size() > 1) ? (float)ElementIndex / (float)(Rod.Elements.size() - 1) : 0.0f;
			float btStiff = KawaiiClamp(Rod.BendAndTwistStiffness * Rod.BendAndTwistStiffnessCurve.Evaluate(btRate), 0.0f, 1.0f);
			float compliance = (1.0f - btStiff) / (dt * dt + KAWAII_SMALL_NUMBER);
			v3dxVector3 correction = darbouxError * (-1.0f / (2.0f + compliance + KAWAII_SMALL_NUMBER));

			// Apply orientation corrections
			float halfAngle0 = Vec3Length(correction) * 0.5f;
			if (halfAngle0 > KAWAII_SMALL_NUMBER)
			{
				v3dxVector3 axis = Vec3SafeNormal(correction);
				float sinH = sinf(halfAngle0);
				v3dxVector3 worldAxis0 = QuatRotateVector(E0.Orientation, axis);
				v3dxQuaternion dq0(worldAxis0.X * sinH, worldAxis0.Y * sinH, worldAxis0.Z * sinH, cosf(halfAngle0));
				E0.Orientation = QuatMultiply(dq0, E0.Orientation);

				v3dxVector3 worldAxis1 = QuatRotateVector(E1.Orientation, axis * -1.0f);
				v3dxQuaternion dq1(worldAxis1.X * sinH, worldAxis1.Y * sinH, worldAxis1.Z * sinH, cosf(halfAngle0));
				E1.Orientation = QuatMultiply(dq1, E1.Orientation);
			}
		}

		void SolvePointAttachmentConstraint(FCosseratRodData& Rod, float dt)
		{
			if (Rod.PointAttachmentStiffness < KAWAII_SMALL_NUMBER) return;

			const size_t ptCount = Rod.Particles.size();
			for (size_t i = 0; i < ptCount; ++i)
			{
				FSimParticle& P = Rod.Particles[i];
				if (P.PinMode != KPM_Dynamic) continue;

				v3dxVector3 error = P.Position - P.PosePosition;
				if (Vec3IsNearlyZero(error)) continue;

				float paRate = (ptCount > 1) ? (float)i / (float)(ptCount - 1) : 0.0f;
				float paStiff = KawaiiClamp(Rod.PointAttachmentStiffness * Rod.PointAttachmentStiffnessCurve.Evaluate(paRate), 0.0f, 1.0f);
				if (paStiff < KAWAII_SMALL_NUMBER) continue;
				float compliance = (1.0f - paStiff) / (dt * dt + KAWAII_SMALL_NUMBER);

				float w = P.InverseMass;
				v3dxVector3 correction = error * (-1.0f / (w + compliance + KAWAII_SMALL_NUMBER));
				P.Position = P.Position + correction * w;
			}
		}

		void SolveOrientationAttachmentConstraint(FCosseratRodData& Rod, float dt)
		{
			if (Rod.OrientationAttachmentStiffness < KAWAII_SMALL_NUMBER) return;

			const size_t oaCount = Rod.Elements.size();
			for (size_t i = 0; i < oaCount; ++i)
			{
				FCosseratRodElement& E = Rod.Elements[i];
				if (E.bIsRoot) continue;

				v3dxQuaternion relQ = QuatMultiply(QuatInverse(E.RestOrientation), E.Orientation);
				if (relQ.W < 0.0f)
				{
					relQ.X = -relQ.X; relQ.Y = -relQ.Y; relQ.Z = -relQ.Z; relQ.W = -relQ.W;
				}

				v3dxVector3 errorAxis(relQ.X, relQ.Y, relQ.Z);
				float errorMag = Vec3Length(errorAxis);
				if (errorMag < KAWAII_SMALL_NUMBER) continue;

				float oaRate = (oaCount > 1) ? (float)i / (float)(oaCount - 1) : 0.0f;
				float oaStiff = KawaiiClamp(Rod.OrientationAttachmentStiffness * Rod.OrientationAttachmentStiffnessCurve.Evaluate(oaRate), 0.0f, 1.0f);
				if (oaStiff < KAWAII_SMALL_NUMBER) continue;
				float compliance = (1.0f - oaStiff) / (dt * dt + KAWAII_SMALL_NUMBER);

				v3dxVector3 correction = errorAxis * (-1.0f / (1.0f + compliance + KAWAII_SMALL_NUMBER));
				float halfAngle = Vec3Length(correction) * 0.5f;
				if (halfAngle > KAWAII_SMALL_NUMBER)
				{
					v3dxVector3 axis = Vec3SafeNormal(correction);
					v3dxVector3 worldAxis = QuatRotateVector(E.Orientation, axis);
					float sinH = sinf(halfAngle);
					v3dxQuaternion dq(worldAxis.X * sinH, worldAxis.Y * sinH, worldAxis.Z * sinH, cosf(halfAngle));
					E.Orientation = QuatMultiply(dq, E.Orientation);
				}
			}
		}

		void SolveRod(FCosseratRodData& Rod, float dt, int32_t Iterations)
		{
			for (int32_t iter = 0; iter < Iterations; ++iter)
			{
				for (int32_t i = 0; i < (int32_t)Rod.Elements.size(); ++i)
					SolveStretchShearConstraint(Rod, i, dt);

				for (int32_t i = 0; i + 1 < (int32_t)Rod.Elements.size(); ++i)
					SolveBendTwistConstraint(Rod, i, dt);

				SolvePointAttachmentConstraint(Rod, dt);
				SolveOrientationAttachmentConstraint(Rod, dt);
			}
		}

		void UpdateElementOrientations(FCosseratRodData& Rod)
		{
			for (size_t i = 0; i < Rod.Elements.size(); ++i)
			{
				Rod.Elements[i].PrevOrientation = Rod.Elements[i].Orientation;

				if (i + 1 < Rod.Particles.size())
				{
					v3dxVector3 edgeDir = Vec3SafeNormal(Rod.Particles[i + 1].Position - Rod.Particles[i].Position);
					v3dxVector3 currentForward = QuatRotateVector(Rod.Elements[i].Orientation, v3dxVector3(0, 0, 1));

					v3dxVector3 crossVec = Vec3Cross(currentForward, edgeDir);
					float dotVal = Vec3Dot(currentForward, edgeDir);
					dotVal = KawaiiClamp(dotVal, -1.0f, 1.0f);

					if (!Vec3IsNearlyZero(crossVec))
					{
						float angle = acosf(dotVal);
						v3dxVector3 axis = Vec3SafeNormal(crossVec);
						float halfAngle = angle * 0.5f;
						float sinH = sinf(halfAngle);
						v3dxQuaternion dq(axis.X * sinH, axis.Y * sinH, axis.Z * sinH, cosf(halfAngle));
						Rod.Elements[i].Orientation = QuatMultiply(dq, Rod.Elements[i].Orientation);
					}
				}
			}
		}

	} // namespace CosseratRodSolver
} // namespace KawaiiPhysics

NS_END
