sampler2D input : register(s0);  //输入的textblock

float w:register(C0);                      //宽度

float h:register(C1);                     //高度

float4 ColorLT:register(C2);    //LT颜色
float4 ColorRT:register(C3);    //RT颜色
float4 ColorLB:register(C4);    //LB颜色
float4 ColorRB:register(C5);    //RB颜色

int OPType:register(C6);		// 描边类型
float OPThickness:register(C7);	// 描边宽度

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	//float3 rgb= bordercolor.rgb ;

	float4 Color; 
	Color= tex2D( input , uv.xy);    //提取当前像素颜色

	//float4 ColorLT = float4(1,0,0,1);
	//float4 ColorLB = float4(0,1,0,1);
	//float4 ColorRT = float4(0,0,1,1);
	//float4 ColorRB = float4(1,1,0,1);

	//float4 tagColor = Color;
	
	float4 tagColor = float4(0,0,0,1);
	//if(Color.a > 0.1)
	{
		tagColor.r = (ColorLT.r * (1 - uv.x) + ColorRT.r * uv.x) * (1 - uv.y) + (ColorLB.r * (1 - uv.x) + ColorRB.r * uv.x) * uv.y;
		tagColor.g = (ColorLT.g * (1 - uv.x) + ColorRT.g * uv.x) * (1 - uv.y) + (ColorLB.g * (1 - uv.x) + ColorRB.g * uv.x) * uv.y;
		tagColor.b = (ColorLT.b * (1 - uv.x) + ColorRT.b * uv.x) * (1 - uv.y) + (ColorLB.b * (1 - uv.x) + ColorRB.b * uv.x) * uv.y;
		tagColor.a = (ColorLT.a * (1 - uv.x) + ColorRT.a * uv.x) * (1 - uv.y) + (ColorLB.a * (1 - uv.x) + ColorRB.a * uv.x) * uv.y;
		//Color.rgb = Color1.rgba * uv.x + Color2.rgba * uv.x;
	}

	Color = tagColor;

	//if(Color.a != 0)
	//	Color = tagColor;
	
	//int i;
	//	
	////if(OPType == 1)
	////{
	////	{
	//		for( i=1;i<2;i++ )                           //修改此循环，可以改动描边的宽度，不过，这里就1次就够了
	//		{
	//			if( Color.a==0   )
	//			{
	//				//float4 cBottom = tex2D( input, uv.xy +float2 (0,i/h) );   // 提取下方像素
	//				//float4 cTop = tex2D( input, uv.xy +float2 (0,-i/h) );    // 提取上方像素 。。。如此这般，共提取4次。
	//				//float4 cLeft = tex2D( input, uv.xy +float2 (-i/w,0) );	// 左
	//				//float4 cRight = tex2D( input, uv.xy +float2 (i/w,0) );	// 右
	//				//float4 cLT = tex2D( input, uv.xy +float2 (-i/w,-i/h) );
	//				//float4 cLB = tex2D( input, uv.xy +float2 (-i/w,i/h) );
	//				//float4 cRT = tex2D( input, uv.xy +float2 (i/w,-i/h) );
	//				//float4 cRB = tex2D( input, uv.xy +float2 (i/w,i/h) );

	//				//if(cBottom.a > 0 || cTop.a > 0 || cLeft.a > 0 || cRight.a > 0 || cLT.a > 0 || cLB.a > 0 || cRT.a > 0 || cRB.a > 0)
	//				//	Color = tagColor;

	//				float ih = 0.01f * i;//i / h;
	//				float iw = 0.01f * i;//i / w;

	//				float4 c2 = tex2D( input, uv.xy +float2 (0,ih) );
	//				if(  c2.a>0 )
	//				{
	//					Color=tagColor;       //描边
	//				}
	//				else
	//				{
	//					c2= tex2D( input, uv.xy +float2 (0,-ih) );    //提取上方像素 。。。如此这般，共提取4次。
	//					if(  c2.a>0 )
	//					{
	//						Color=tagColor;
	//					}
	//					else
	//					{
	//						c2= tex2D( input, uv.xy +float2 (iw,0) );
	//						if(  c2.a>0 )
	//						{
	//							Color=tagColor;
	//						}
	//						else
	//						{
	//							c2= tex2D( input, uv.xy +float2 (-iw,0) );
	//							if(  c2.a>0 )
	//							{
	//								Color=tagColor;
	//							}
	//							else
	//							{
	//								c2 = tex2D( input, uv.xy + float2(-iw, -ih));
	//								if(c2.a > 0)
	//								{
	//									Color=tagColor;
	//								}
	//								else
	//								{
	//									c2 = tex2D( input, uv.xy + float2(-iw, ih));
	//									if(c2.a > 0)
	//									{
	//										Color=tagColor;
	//									}
	//									else
	//									{
	//										c2 = tex2D( input, uv.xy + float2(iw, -ih));
	//										if(c2.a > 0)
	//										{
	//											Color=tagColor;
	//										}
	//										else
	//										{
	//											c2 = tex2D( input, uv.xy + float2(iw, ih));
	//											if(c2.a > 0)
	//											{
	//												Color=tagColor;
	//											}
	//										}
	//									}
	//								}
	//							}
	//						}
	//					}
	//				}
	//			}
	//			else
	//			{
	//				//float4 tempcolor = Color;
	//				//
	//				//
	//				//if( Color.a<0.1)            //由于SL对字体的反锯齿处理，这里有一个非常讨厌的现象：有半透明像素存在，经过测试，如果透明度在0.1一下，则判断为边，否则为字。
	//				//{
	//				//	tempcolor=tagColor;
	//				//}
	//				//
	//				//Color=tempcolor;
	//				Color = tagColor;
	//			}
	//		}
	////	}
	////}
	//// INNER
	////switch(OPType)
	////{
	////case 0:
	////	break;

	////case 1:

	////	break;
	////	
	////case 2:
	////	break;

	////case 3:
	////	break;

	////default:
	////	break;
	////}	


	return Color; 
}

