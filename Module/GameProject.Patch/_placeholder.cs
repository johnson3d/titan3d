// 补丁工程占位文件。
// 需要热更时，在此目录下创建补丁类，继承镜像桩并用 new 覆盖方法。
// 示例：
//
// namespace NS_tutorials.helloword
// {
//     public class helloword__Patch : global::NS_tutorials.helloword
//     {
//         public new System.Boolean Foo(System.Int32 a) { /* 修正后的逻辑 */ return true; }
//     }
// }
