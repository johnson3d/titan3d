using EngineNS.UnitTest;

namespace EngineNS.Graphics.Pipeline
{
    [TtTest]
    public class UTest_RenderGraph
    {
        public void UnitTestEntrance()
        {
            using (var graph = new TtRenderGraph())
            {
                var source = new Common.TtCopyNode();
                var pass = new Common.TtCopyNode();
                var unused = new Common.TtCopyNode();
                var root = new Common.TtAssitRootNode();

                TtUnitTestManager.TAssert(graph.RegRenderNode2("Source", source),
                    "RenderGraph should register the source node");
                TtUnitTestManager.TAssert(graph.RegRenderNode2("Pass", pass),
                    "RenderGraph should register the pass node");
                TtUnitTestManager.TAssert(graph.RegRenderNode2("Unused", unused),
                    "RenderGraph should register the unused node");
                TtUnitTestManager.TAssert(graph.RegRenderNode2("Root", root),
                    "RenderGraph should register the root node");

                source.InitNodePins();
                pass.InitNodePins();
                unused.InitNodePins();
                root.InitNodePins();
                source.SrcPinIn.IsAllowInputNull = true;

                TtUnitTestManager.TAssert(graph.AddLinker(source.DestPinOut, pass.SrcPinIn),
                    "RenderGraph should link source output to pass input");
                TtUnitTestManager.TAssert(graph.AddLinker(pass.DestPinOut, root.SrcPinIn),
                    "RenderGraph should link pass output to root input");
                graph.RootNode = root;

                bool hasInputError = false;
                graph.BuildGraph(ref hasInputError);

                TtUnitTestManager.TAssert(!hasInputError,
                    "A fully connected RenderGraph should not report missing inputs");
                TtUnitTestManager.TAssert(source.IsUsed && pass.IsUsed && root.IsUsed,
                    "Every node reachable from the root should be retained");
                TtUnitTestManager.TAssert(!unused.IsUsed,
                    "A branch with no path to an ending node should be pruned");

                TtUnitTestManager.TAssert(source.MaxLeafDistance == 0,
                    $"Source should be in topology layer 0, actual={source.MaxLeafDistance}");
                TtUnitTestManager.TAssert(pass.MaxLeafDistance == 1,
                    $"Pass should be in topology layer 1, actual={pass.MaxLeafDistance}");
                TtUnitTestManager.TAssert(root.MaxLeafDistance == 2,
                    $"Root should be in topology layer 2, actual={root.MaxLeafDistance}");
                TtUnitTestManager.TAssert(graph.NodeLayers != null && graph.NodeLayers.Length == 3,
                    "A three-stage dependency chain should produce three topology layers");
                TtUnitTestManager.TAssert(graph.NodeLayers[0].Contains(source) &&
                    graph.NodeLayers[1].Contains(pass) && graph.NodeLayers[2].Contains(root),
                    "Topology layers should order producers before their consumers");

                TtUnitTestManager.TAssert(pass.SrcPinIn.Attachement.AttachmentName.Equals(
                        source.DestPinOut.Attachement.AttachmentName),
                    "A linked input should inherit its producer attachment identity");
                TtUnitTestManager.TAssert(root.SrcPinIn.Attachement.AttachmentName.Equals(
                        pass.DestPinOut.Attachement.AttachmentName),
                    "Attachment identity should propagate through the complete chain");

                var imported = new TtAttachBuffer();
                var cached = graph.AttachmentCache.ImportAttachment(source.DestPinOut, imported);
                var reused = graph.AttachmentCache.ImportAttachment(source.DestPinOut, null);
                TtUnitTestManager.TAssert(object.ReferenceEquals(imported, cached) &&
                    object.ReferenceEquals(cached, reused),
                    "Importing the same pin twice should reuse its cached attachment");

                cached.AddRef(2);
                TtUnitTestManager.TAssert(cached.RefCount == 2,
                    $"Two consumers should produce two attachment references, actual={cached.RefCount}");
                TtUnitTestManager.TAssert(cached.Release() == 1 && cached.Release() == 0,
                    "Attachment references should be released once per consumer");

                graph.AttachmentCache.RemoveAttachement(source.DestPinOut.Attachement.AttachmentName);
                TtUnitTestManager.TAssert(graph.AttachmentCache.FindAttachement(source.DestPinOut) == null,
                    "Removing an imported attachment should remove its cache entry without allocating GPU resources");
            }
        }
    }
}
