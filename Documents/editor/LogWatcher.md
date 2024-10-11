- ��־�鿴
- - ѡ��checkboxɸѡ��ʾ����־
  - Category���Խ�һ��ɸѡ
- Command
	- ����ͼ���ͷ������������
    - ![ͼƬ](LogWatcher/LogWatcher.png)
	- �����ʽ CommandName ArgName1=XXXX ArgName2=YYYY
	- ��������
	- - C#������չ
    ```C#
        public class TtCommand_DelMaterialShaderCache : TtCommand
        {
            public TtCommand_DelMaterialShaderCache()
            {
                CmdName = "DelMaterialCache";
                CmdHelp = "DelMaterialCache Material={string}";
            }
            public override void Execute(string argsText)
            {
                _ = ExecuteImpl(argsText);
            }
            private async Thread.Async.TtTask ExecuteImpl(string argsText)
            {
                var args = GetArguments(argsText);
                var arg = FindArgument(args, "Material");
                var mtlName = GetRNameByArg(arg, null);
                var mtl = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(mtlName);
                if (mtl == null)
                    return;
                var files = IO.TtFileManager.GetFiles(TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Effect), "*.effect", true);
                foreach (var i in files)
                {
                    var desc = Graphics.Pipeline.Shader.TtEffect.LoadEffectDesc(i);
                    if (desc.MaterialHash == mtl.MaterialHash)
                    {
                        IO.TtFileManager.DeleteFile(i);
                    }
                }
            }
        }
    ```
	- - ��ͼ��չ
      - ����һ��TtCommandMacross�����ս̳�[McCmd](../tutorials/mc_cmd.md)����
	- ���ó�������
    - - ����һ����ͼ���� McCmd Macross=(string) OnGameThread=(bool)
      - ��ʾ���������б� List Filter=(string)
      - ��ʾ������� Help Cmd=(string)
      - ��ӡRenderGraph�ĳ���Ϣ PrintAttachmentPool
      - ɾ��ָ�����ʵ�ShaderCache DelMaterialCache Material={string}