#include "NxDevice.h"
#include "NxScene.h"
#include "NxShape.h"
#include "NxActor.h"
#include "NxJoint.h"

NS_BEGIN

namespace NxPhysics
{
	NxScene* NxDevice::CreateScene(const NxSceneDesc* desc)
	{
		auto result = new NxScene();
		if (result->Init(*desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	NxRigidBody* NxDevice::CreateRigidBody(const NxRigidBodyDesc* desc)
	{
		return new NxRigidBody();
	}
	NxSphereShape* NxDevice::CreateSphereShape(const NxSphereShapeDesc* desc)
	{
		auto result = new NxSphereShape();
		if (result->Init(*desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}

	void Test()
	{
		auto device = MakeShared(new NxDevice());
		NxSphereShapeDesc sphereDesc{};
		auto sphereShape = MakeShared(device->CreateSphereShape(&sphereDesc));
		NxRigidBodyDesc rbDesc{};
		auto rigidBody = MakeShared(device->CreateRigidBody(&rbDesc));
		rigidBody->AddShape(sphereShape);
		NxSceneDesc sceneDesc{};
		auto scene = MakeShared(device->CreateScene(&sceneDesc));
		scene->AddActor(rigidBody);

		NxReal elapsed = NxReal(0.01f);
		while (true)
		{
			scene->Simulate(elapsed);

			for (auto& i : scene->mActors)
			{
				if (i->GetRtti() == GetClassObject<NxRigidBody>())
				{
					auto pBody = (NxRigidBody*)i;
					const auto* pTransform = pBody->GetTransform();
					pTransform->Position;
				}
			}
		}
	}
}

NS_END