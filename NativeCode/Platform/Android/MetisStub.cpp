// Android 侧没有可用的 METIS 实体：3rd/native/metis/5.1.0 只随仓库提供 Windows/Mac/
// x86_64-Linux 产物，也没有源码可现场编译 arm64。这里保留 METIS 的 ABI：默认选项照常
// 填充，划分请求返回 METIS_ERROR_INPUT，让 Quark 的 FGraphPartitioner 走“不支持”分支
// 放弃聚簇（退出时 Clusters 为空），而不是链接失败、崩溃或静默产出无效分区。

#include "../../../3rd/native/metis/5.1.0/include/metis.h"

extern "C" {

int METIS_SetDefaultOptions(idx_t* options)
{
	if (options == nullptr)
		return METIS_ERROR_INPUT;
	for (int i = 0; i < METIS_NOPTIONS; i++)
		options[i] = -1;
	return METIS_OK;
}

int METIS_PartGraphRecursive(idx_t* nvtxs, idx_t* ncon, idx_t* xadj,
	idx_t* adjncy, idx_t* vwgt, idx_t* vsize, idx_t* adjwgt,
	idx_t* nparts, real_t* tpwgts, real_t* ubvec, idx_t* options,
	idx_t* edgecut, idx_t* part)
{
	(void)nvtxs; (void)ncon; (void)xadj; (void)adjncy; (void)vwgt; (void)vsize;
	(void)adjwgt; (void)nparts; (void)tpwgts; (void)ubvec; (void)options;
	(void)edgecut; (void)part;
	return METIS_ERROR_INPUT;
}

int METIS_PartGraphKway(idx_t* nvtxs, idx_t* ncon, idx_t* xadj,
	idx_t* adjncy, idx_t* vwgt, idx_t* vsize, idx_t* adjwgt,
	idx_t* nparts, real_t* tpwgts, real_t* ubvec, idx_t* options,
	idx_t* edgecut, idx_t* part)
{
	(void)nvtxs; (void)ncon; (void)xadj; (void)adjncy; (void)vwgt; (void)vsize;
	(void)adjwgt; (void)nparts; (void)tpwgts; (void)ubvec; (void)options;
	(void)edgecut; (void)part;
	return METIS_ERROR_INPUT;
}

} // extern "C"
