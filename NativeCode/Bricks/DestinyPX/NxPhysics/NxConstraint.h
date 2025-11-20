#pragma once

#include "NxActor.h"

NS_BEGIN

namespace NxPhysics
{
	class NxConstraint : public NxEntity
	{
	public:
		ENGINE_RTTI(NxConstraint);
		virtual const NxPQ* GetTransform() const override {
			return nullptr;
		}
		virtual NxPQ* GetTransform() override {
			return nullptr;
		}
		virtual void SolveConstraint(NxScene* scene, const NxReal& time) = 0;
	};

    namespace XPBD 
    {
        // ---------------------------- 粒子数据结构 ----------------------------
        template <typename T>
        struct Particle {
            T position;     // 位置（2D/3D向量）
            T prevPosition; // 上一帧位置（用于Verlet积分）
            T velocity;     // 速度
            NxReal mass;     // 质量
            NxReal invMass;  // 质量倒数（XPBD中常用）

            Particle(const T& pos, NxReal m = 1.0f)
                : position(pos), prevPosition(pos), velocity(T()), mass(m), invMass(1.0f / m) {
            }
        };

        // ---------------------------- 约束基类（泛型） ----------------------------
        template <typename T>
        class Constraint {
        public:
            virtual ~Constraint() = default;

            // 计算约束值 C(x)
            virtual NxReal evaluate(const std::vector<Particle<T>>& particles) const = 0;

            // 计算梯度 ∇C（对每个粒子的偏导）
            virtual std::vector<T> computeGradients(const std::vector<Particle<T>>& particles) const = 0;

            // 返回受约束影响的粒子索引
            virtual const std::vector<size_t>& getIndices() const = 0;

            // XPBD的约束刚度（可动态调整）
            NxReal stiffness = 1.0f;
            NxReal compliance = 1.0f / stiffness; // 柔度（α的倒数）
        };

        // ---------------------------- XPBD引擎 ----------------------------
        template <typename T>
        class XPBDEngine {
        private:
            std::vector<Particle<T>> particles;
            std::vector<std::unique_ptr<Constraint<T>>> constraints;
            NxReal timeStep = 1.0f / 60.0f; // 时间步长

        public:
            // 添加粒子
            void addParticle(const T& pos, float mass = 1.0f) {
                particles.emplace_back(pos, mass);
            }

            // 添加约束
            template <typename C, typename... Args>
            void addConstraint(Args&&... args) {
                constraints.push_back(std::make_unique<C>(std::forward<Args>(args)...));
            }

            // 主更新逻辑（Verlet积分 + XPBD约束求解）
            void update() {
                // 1. Verlet积分预测位置
                for (auto& p : particles) {
                    if (p.invMass > NxReal::Zero()) {
                        T temp = p.position;
                        p.position += (p.position - p.prevPosition) + p.velocity * timeStep;
                        p.prevPosition = temp;
                    }
                }

                // 2. XPBD约束迭代
                const int solverIterations = 3;
                for (int iter = 0; iter < solverIterations; ++iter) {
                    for (const auto& constraint : constraints) {
                        // 2.1 计算约束值 C(x) 和梯度 ∇C
                        auto C = constraint->evaluate(particles);
                        std::vector<T> gradients = constraint->computeGradients(particles);
                        const auto& indices = constraint->getIndices();

                        // 2.2 计算拉格朗日乘数增量 Δλ
                        NxReal sumGradSq = 0.0f;
                        NxReal totalInvMass = 0.0f;
                        for (size_t i = 0; i < indices.size(); ++i) {
                            sumGradSq += NxVector3::Dot(gradients[i],gradients[i]); // ‖∇C‖²
                            totalInvMass += particles[indices[i]].invMass;
                        }

                        auto deltaLambda = (-C) /
                            (sumGradSq + constraint->compliance / (timeStep * timeStep));

                        // 2.3 更新粒子位置
                        for (size_t i = 0; i < indices.size(); ++i) {
                            particles[indices[i]].position += gradients[i] * (deltaLambda * particles[indices[i]].invMass);
                        }
                    }
                }

                // 3. 更新速度（可选）
                for (auto& p : particles) {
                    p.velocity = (p.position - p.prevPosition) / timeStep;
                }
            }

            // 获取粒子数据（用于渲染/调试）
            const std::vector<Particle<T>>& getParticles() const { return particles; }
        };

        // ---------------------------- 具体约束示例：距离约束 ----------------------------
        template <typename T>
        class DistanceConstraint : public Constraint<T> {
        private:
            std::vector<size_t> indices; // 粒子索引（2个）
            NxReal restLength;            // 初始距离

        public:
            DistanceConstraint(size_t i1, size_t i2, NxReal length)
                : indices({ i1, i2 }), restLength(length) {
            }

            const std::vector<size_t>& getIndices() const override { return indices; }

            NxReal evaluate(const std::vector<Particle<T>>& particles) const override {
                const T& p1 = particles[indices[0]].position;
                const T& p2 = particles[indices[1]].position;
                return (p1 - p2).Length() - restLength; // C(x) = |x1 - x2| - d
            }

            std::vector<T> computeGradients(const std::vector<Particle<T>>& particles) const override {
                const T& p1 = particles[indices[0]].position;
                const T& p2 = particles[indices[1]].position;
                T delta = p1 - p2;
                NxReal length = delta.Length();
                T grad1 = (length > NxReal::Zero()) ? delta / length : T(); // ∇C1 = (x1 - x2)/|x1 - x2|
                T grad2 = -grad1;                               // ∇C2 = -∇C1
                return { grad1, grad2 };
            }
        };
    }
}

NS_END