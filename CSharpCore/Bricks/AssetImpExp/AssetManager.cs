using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AssetImpExp
{
    public class AssetManager
    {
        FBX.CGfxFBXImporter mFBXImporter = new FBX.CGfxFBXImporter();
        //预读？
        public void PreImport(string[] fileNames,ref CGfxFileImportOption assetImportOption)
        {
            if(fileNames.Length  ==1 )
            {
                mFBXImporter.PreImport(fileNames[0], ref assetImportOption);
            }
            else
            {
                //使用通用AssetImportOption
                for(int i = 0; i< fileNames.Length;i++)
                {

                }
            }
        }
        public void Import(string[] fileNames, ref CGfxFileImportOption assetImportOption)
        {
            if (fileNames.Length == 1)
            {
                mFBXImporter.Import(fileNames[0]);
            }
            else
            {
                //使用通用AssetImportOption
                for (int i = 0; i < fileNames.Length; i++)
                {

                }
            }
        }

    }
}
