#include "NxConstraint.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxConstraint);

	namespace XPBD 
	{
        // ---------------------------- 主函数测试 ----------------------------
        int test() {
            // 1. 初始化引擎
            XPBDEngine<NxVector3> engine;

            // 2. 添加粒子
            engine.addParticle(NxVector3(0, 0, 0), 1.0f); // 粒子0（固定）
            engine.addParticle(NxVector3(0.5, 0, 0), 1.0f); // 粒子1

            // 3. 添加距离约束
            engine.addConstraint<DistanceConstraint<NxVector3>>(0, 1, 1.0f);

            // 4. 模拟循环
            for (int step = 0; step < 100; ++step) {
                engine.update();
                const auto& particles = engine.getParticles();
                std::cout << "Step " << step << ": Particle1 at ("
                    << particles[1].position.X << ", "
                    << particles[1].position.Y << ")\n";
            }

            return 0;
        }
	}
}

NS_END