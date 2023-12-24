#include "NxDevice.h"
#include "NxScene.h"
#include "NxShape.h"
#include "NxActor.h"
#include "NxJoint.h"

NS_BEGIN

namespace NxPhysics
{
	template<typename T, typename D>
	T* CreateAndInitObject(const D& desc)
	{
		auto result = new T();
		if (result->Init(desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	NxScene* NxDevice::CreateScene(const NxSceneDesc* desc)
	{
		return CreateAndInitObject<NxScene>(*desc);
	}
	NxRigidBody* NxDevice::CreateRigidBody(const NxRigidBodyDesc* desc)
	{
		return CreateAndInitObject<NxRigidBody>(*desc);
	}
	NxSphereShape* NxDevice::CreateSphereShape(const NxSphereShapeDesc* desc)
	{
		return CreateAndInitObject<NxSphereShape>(*desc);
	}
	NxBoxShape* NxDevice::CreateBoxShape(const NxBoxShapeDesc* desc)
	{
		return CreateAndInitObject<NxBoxShape>(*desc);
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