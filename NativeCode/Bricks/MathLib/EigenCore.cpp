#include "EigenCore.h"
#include <iostream>

#pragma warning(push)
#pragma warning(disable:4819)
#include <Eigen/Eigen>
#pragma warning(pop)

NS_BEGIN

void EigenUtility::TestJacobi()
{
	//https://zhuanlan.zhihu.com/p/30965284
	int max1 = 300;
	float tol = 0.00001f;
	Eigen::Vector<float, 3> x0(0, 0, 0);
	Eigen::Vector<float, 3> b(20, 33, 12);
	Eigen::Matrix<float, 3, 3> A;
	A << 8, -3, 2,
		4, 11, -1,
		2, 1, 4;

	/*A(0, 0) = 8;
	A(0, 1) = -3;
	A(0, 2) = 2;

	A(1, 0) = 4;
	A(1, 1) = 11;
	A(1, 2) = -1;

	A(2, 0) = 2;
	A(2, 1) = 1;
	A(2, 2) = 4;*/

	std::cout << A << std::endl;

	auto D = Eigen::Matrix<float, 3, 3>(A.diagonal().array().matrix().asDiagonal());
	std::cout << "D=>" << std::endl;
	std::cout << D <<std::endl;
	auto U = -Eigen::Matrix<float, 3, 3>(A.triangularView<Eigen::UpLoType::Upper | Eigen::UpLoType::ZeroDiag>());
	std::cout << "U=>" << std::endl;
	std::cout << U << std::endl;
	auto L = -Eigen::Matrix<float, 3, 3>(A.triangularView<Eigen::UpLoType::Lower | Eigen::UpLoType::ZeroDiag>());
	std::cout << "L=>" << std::endl;
	std::cout << L << std::endl;
	
	auto invD = D.inverse();
	auto B = invD * (L + U);
	std::cout << "B=>" << std::endl;
	std::cout << B << std::endl;
	auto f = invD * b;
	std::cout << "f=>" << std::endl;
	std::cout << f << std::endl;

	Eigen::Vector<float, 3> x = B * x0 + f;
	int k = 1;

	float n = (x - x0).norm();
	while (n >= tol)
	{
		x0 = x;
		x = B * x0 + f;
		k++;
		if (k >= max1)
		{
			break;
		}
		n = (x - x0).norm();
	}
	std::cout << "K = " << k << std::endl;
	std::cout << x << std::endl;
}

NS_END