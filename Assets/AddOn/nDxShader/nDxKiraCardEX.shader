// Shader created with Shader Forge v1.40 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.40;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,cpap:True,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:False,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:False,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:36002,y:31790,varname:node_2865,prsc:2|diff-3110-OUT,spec-6955-OUT,gloss-516-OUT,normal-9435-OUT,emission-8173-OUT;n:type:ShaderForge.SFN_Tex2d,id:7736,x:32913,y:33441,ptovrint:True,ptlb:Art Tex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:179154ba93fe3314383193d2ba30f753,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5465,x:33520,y:33990,ptovrint:False,ptlb:Frame Normal,ptin:_FrameNormal,varname:node_5465,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:aa3265674c1c28d4bb915004242baf0e,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Color,id:5675,x:33362,y:33203,ptovrint:False,ptlb:Frame Color,ptin:_FrameColor,varname:node_5675,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.8970588,c2:0.7652294,c3:0.4815095,c4:1;n:type:ShaderForge.SFN_Lerp,id:5783,x:33783,y:33451,varname:node_5783,prsc:2|A-8392-OUT,B-7457-OUT,T-6678-R;n:type:ShaderForge.SFN_Tex2d,id:6678,x:33520,y:33814,ptovrint:False,ptlb:Frame Tex,ptin:_FrameTex,varname:node_6678,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:be0038d502cb9c142aab8b2dfd270946,ntxv:0,isnm:False;n:type:ShaderForge.SFN_FaceSign,id:8220,x:35472,y:31984,varname:node_8220,prsc:2,fstp:0;n:type:ShaderForge.SFN_Tex2d,id:9704,x:33708,y:34765,ptovrint:False,ptlb:BackFace,ptin:_BackFace,varname:node_9704,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c6636bf7ae35bc1448a1f93e10e7b5af,ntxv:0,isnm:False|UVIN-1766-OUT;n:type:ShaderForge.SFN_Lerp,id:3110,x:35685,y:31681,varname:node_3110,prsc:2|A-1906-OUT,B-423-OUT,T-8220-VFACE;n:type:ShaderForge.SFN_Lerp,id:516,x:35685,y:31954,varname:node_516,prsc:2|A-6255-OUT,B-114-OUT,T-8220-VFACE;n:type:ShaderForge.SFN_Lerp,id:9435,x:35685,y:32129,varname:node_9435,prsc:2|A-6142-OUT,B-8803-OUT,T-8220-VFACE;n:type:ShaderForge.SFN_Lerp,id:6955,x:35685,y:31832,varname:node_6955,prsc:2|A-6255-OUT,B-5615-OUT,T-8220-VFACE;n:type:ShaderForge.SFN_TexCoord,id:2582,x:33099,y:34784,varname:node_2582,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:7245,x:33326,y:34933,varname:node_7245,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-2582-V;n:type:ShaderForge.SFN_OneMinus,id:3155,x:33326,y:34765,varname:node_3155,prsc:2|IN-2582-U;n:type:ShaderForge.SFN_Append,id:1766,x:33527,y:34765,varname:node_1766,prsc:2|A-3155-OUT,B-7245-OUT;n:type:ShaderForge.SFN_UVTile,id:1243,x:32504,y:32523,varname:node_1243,prsc:2|UVIN-9774-UVOUT,WDT-3278-OUT,HGT-3278-OUT,TILE-2093-OUT;n:type:ShaderForge.SFN_TexCoord,id:1946,x:31779,y:32353,varname:node_1946,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Slider,id:3278,x:32083,y:32602,ptovrint:False,ptlb:Kira Tile,ptin:_KiraTile,varname:_KiraTile_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5555556,max:1;n:type:ShaderForge.SFN_Vector1,id:2093,x:32115,y:32804,varname:node_2093,prsc:2,v1:1;n:type:ShaderForge.SFN_Tex2d,id:2407,x:32962,y:32953,ptovrint:False,ptlb:Sub Kira Tex,ptin:_SubKiraTex,varname:_SubNormal_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:7df63a4247a77c549a2f511374372e64,ntxv:3,isnm:True|UVIN-6548-UVOUT;n:type:ShaderForge.SFN_Rotator,id:9774,x:32068,y:32385,varname:node_9774,prsc:2|UVIN-1946-UVOUT,ANG-932-OUT;n:type:ShaderForge.SFN_Slider,id:2550,x:31483,y:32530,ptovrint:False,ptlb:Kira Angle,ptin:_KiraAngle,varname:_KiraAngle_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2368918,max:2;n:type:ShaderForge.SFN_NormalBlend,id:3827,x:33534,y:32747,varname:node_3827,prsc:2|BSE-3254-OUT,DTL-7587-OUT;n:type:ShaderForge.SFN_TexCoord,id:96,x:32383,y:32702,varname:node_96,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_UVTile,id:6548,x:32764,y:32953,varname:node_6548,prsc:2|UVIN-2263-UVOUT,WDT-4835-OUT,HGT-4835-OUT,TILE-2093-OUT;n:type:ShaderForge.SFN_Slider,id:4835,x:32736,y:33201,ptovrint:False,ptlb:Sub Kira Tile,ptin:_SubKiraTile,varname:_SubKiraTile_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:9841,x:32792,y:33619,ptovrint:False,ptlb:Kira Metal,ptin:_KiraMetal,varname:_KiraMetal_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:5187,x:32772,y:33834,ptovrint:False,ptlb:Kira Gloss,ptin:_KiraGloss,varname:_KiraGloss_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7264957,max:1;n:type:ShaderForge.SFN_Tex2d,id:8076,x:33422,y:32032,varname:node_3136,prsc:2,tex:0b3677ea3bd08c4408b9ea5a4bdf433e,ntxv:0,isnm:False|UVIN-2449-OUT,TEX-4785-TEX;n:type:ShaderForge.SFN_Transform,id:1276,x:33029,y:32032,varname:node_1276,prsc:2,tffrom:0,tfto:3|IN-7952-OUT;n:type:ShaderForge.SFN_NormalVector,id:1467,x:31711,y:32105,prsc:2,pt:False;n:type:ShaderForge.SFN_ComponentMask,id:2449,x:33207,y:32032,varname:node_2449,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-1276-XYZ;n:type:ShaderForge.SFN_Dot,id:2120,x:31912,y:32032,varname:node_2120,prsc:2,dt:0|A-262-OUT,B-1467-OUT;n:type:ShaderForge.SFN_Vector3,id:6968,x:31081,y:32418,varname:node_6968,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Lerp,id:3254,x:33329,y:32584,varname:node_3254,prsc:2|A-5014-OUT,B-1429-OUT,T-7089-OUT;n:type:ShaderForge.SFN_Slider,id:7089,x:32849,y:32712,ptovrint:False,ptlb:Kira Power,ptin:_KiraPower,varname:node_4590,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:1814,x:32362,y:32032,varname:node_1814,prsc:2|A-1037-OUT,B-7737-OUT;n:type:ShaderForge.SFN_Slider,id:7737,x:32133,y:32215,ptovrint:False,ptlb:Holo Shift,ptin:_HoloShift,varname:node_8354,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2,max:2;n:type:ShaderForge.SFN_AmbientLight,id:6067,x:31069,y:32904,varname:node_6067,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:749,x:32744,y:32519,varname:node_9884,prsc:2,tex:876e4176d30bee54db2ca97158d72e55,ntxv:3,isnm:True|UVIN-1243-UVOUT,TEX-8816-TEX;n:type:ShaderForge.SFN_Blend,id:1429,x:32971,y:32519,varname:node_1429,prsc:2,blmd:5,clmp:True|SRC-749-RGB,DST-749-RGB;n:type:ShaderForge.SFN_Blend,id:7686,x:33665,y:32032,varname:node_7686,prsc:2,blmd:10,clmp:True|SRC-8076-RGB,DST-7141-OUT;n:type:ShaderForge.SFN_Slider,id:7141,x:33448,y:32267,ptovrint:False,ptlb:Holo Brightness,ptin:_HoloBrightness,varname:node_2159,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_Color,id:2657,x:34280,y:32198,ptovrint:False,ptlb:Kira Color,ptin:_KiraColor,varname:node_3475,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_ComponentMask,id:5401,x:34101,y:32031,varname:node_5401,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-7686-OUT;n:type:ShaderForge.SFN_Add,id:5297,x:34291,y:32041,varname:node_5297,prsc:2|A-5401-OUT,B-5401-B;n:type:ShaderForge.SFN_Set,id:9643,x:33737,y:32747,varname:KiraNormal,prsc:2|IN-3827-OUT;n:type:ShaderForge.SFN_Get,id:9886,x:32772,y:34076,varname:node_9886,prsc:2|IN-9643-OUT;n:type:ShaderForge.SFN_Get,id:8213,x:32801,y:32228,varname:node_8213,prsc:2|IN-9643-OUT;n:type:ShaderForge.SFN_Multiply,id:9770,x:34488,y:32177,varname:node_9770,prsc:2|A-5297-OUT,B-2657-RGB;n:type:ShaderForge.SFN_Tex2d,id:4446,x:32590,y:32032,varname:node_4446,prsc:2,tex:0b3677ea3bd08c4408b9ea5a4bdf433e,ntxv:0,isnm:False|UVIN-1814-OUT,TEX-4785-TEX;n:type:ShaderForge.SFN_NormalBlend,id:9398,x:32822,y:32032,varname:node_9398,prsc:2|BSE-4446-RGB,DTL-8213-OUT;n:type:ShaderForge.SFN_Set,id:6029,x:34666,y:32164,varname:KiraAlbedo,prsc:2|IN-9770-OUT;n:type:ShaderForge.SFN_Get,id:1575,x:32913,y:33361,varname:node_1575,prsc:2|IN-6029-OUT;n:type:ShaderForge.SFN_Append,id:1037,x:32147,y:32032,varname:node_1037,prsc:2|A-2120-OUT,B-2120-OUT;n:type:ShaderForge.SFN_Tex2d,id:8865,x:32448,y:31569,varname:node_8865,prsc:2,tex:876e4176d30bee54db2ca97158d72e55,ntxv:3,isnm:True|UVIN-1243-UVOUT,TEX-8816-TEX;n:type:ShaderForge.SFN_NormalBlend,id:7952,x:33029,y:31807,varname:node_7952,prsc:2|BSE-9398-OUT,DTL-4708-OUT;n:type:ShaderForge.SFN_Slider,id:5152,x:32362,y:31861,ptovrint:False,ptlb:Card Distortion,ptin:_CardDistortion,varname:node_5152,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Lerp,id:4708,x:32732,y:31726,varname:node_4708,prsc:2|A-512-OUT,B-8865-RGB,T-5152-OUT;n:type:ShaderForge.SFN_Cubemap,id:8652,x:31098,y:32646,ptovrint:False,ptlb:SkyBox,ptin:_SkyBox,varname:node_8652,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,cube:1100cd5ea5005034dab121cd04d64348,pvfc:0;n:type:ShaderForge.SFN_Lerp,id:7587,x:33347,y:32948,varname:node_7587,prsc:2|A-5014-OUT,B-2407-RGB,T-7958-OUT;n:type:ShaderForge.SFN_Slider,id:7958,x:32849,y:32838,ptovrint:False,ptlb:Sub Kira Power,ptin:_SubKiraPower,varname:node_7958,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Set,id:144,x:31345,y:32445,varname:PlaneNormal,prsc:2|IN-6968-OUT;n:type:ShaderForge.SFN_Get,id:5014,x:33123,y:32773,varname:node_5014,prsc:2|IN-144-OUT;n:type:ShaderForge.SFN_Get,id:512,x:32472,y:31707,varname:node_512,prsc:2|IN-144-OUT;n:type:ShaderForge.SFN_Pi,id:429,x:31709,y:32608,varname:node_429,prsc:2;n:type:ShaderForge.SFN_Multiply,id:932,x:31890,y:32498,varname:node_932,prsc:2|A-2550-OUT,B-429-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:4785,x:33026,y:32297,ptovrint:False,ptlb:Holo Tex,ptin:_HoloTex,varname:node_4785,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:0b3677ea3bd08c4408b9ea5a4bdf433e,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2dAsset,id:8816,x:31085,y:32055,ptovrint:False,ptlb:Kira Tex,ptin:_KiraTex,varname:node_8816,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:876e4176d30bee54db2ca97158d72e55,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Color,id:9371,x:34178,y:32689,ptovrint:False,ptlb:SkyBox Color,ptin:_SkyBoxColor,varname:node_9371,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_TexCoord,id:3683,x:34020,y:32822,varname:node_3683,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:8007,x:34376,y:32965,varname:node_8007,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-1452-UVOUT;n:type:ShaderForge.SFN_Rotator,id:1452,x:34222,y:32888,varname:node_1452,prsc:2|UVIN-3683-UVOUT,ANG-5872-OUT;n:type:ShaderForge.SFN_Slider,id:7507,x:33683,y:32966,ptovrint:False,ptlb:Grad Angle,ptin:_GradAngle,varname:node_7507,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:7806,x:33796,y:33105,varname:node_7806,prsc:2;n:type:ShaderForge.SFN_Multiply,id:5872,x:34042,y:33014,varname:node_5872,prsc:2|A-7507-OUT,B-7806-OUT;n:type:ShaderForge.SFN_Lerp,id:1743,x:34624,y:33003,varname:node_1743,prsc:2|A-9371-RGB,B-8034-RGB,T-8007-OUT;n:type:ShaderForge.SFN_Color,id:8034,x:34265,y:33168,ptovrint:False,ptlb:Grad Color,ptin:_GradColor,varname:node_8034,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_SwitchProperty,id:7484,x:34624,y:32810,ptovrint:False,ptlb:Gradation,ptin:_Gradation,varname:node_7484,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-9371-RGB,B-1743-OUT;n:type:ShaderForge.SFN_Lerp,id:94,x:33203,y:33462,varname:node_94,prsc:2|A-1575-OUT,B-7736-RGB,T-7736-A;n:type:ShaderForge.SFN_Lerp,id:5349,x:33203,y:33646,varname:node_5349,prsc:2|A-9841-OUT,B-762-OUT,T-7736-A;n:type:ShaderForge.SFN_Slider,id:762,x:32772,y:33723,ptovrint:False,ptlb:Art Metal,ptin:_ArtMetal,varname:node_762,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Lerp,id:7025,x:33203,y:33795,varname:node_7025,prsc:2|A-5187-OUT,B-2145-OUT,T-7736-A;n:type:ShaderForge.SFN_Slider,id:2145,x:32772,y:33954,ptovrint:False,ptlb:Art Gloss,ptin:_ArtGloss,varname:node_2145,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2136752,max:1;n:type:ShaderForge.SFN_Lerp,id:3910,x:33203,y:33947,varname:node_3910,prsc:2|A-9886-OUT,B-2384-OUT,T-7736-A;n:type:ShaderForge.SFN_Get,id:2384,x:32772,y:34165,varname:node_2384,prsc:2|IN-144-OUT;n:type:ShaderForge.SFN_Lerp,id:3288,x:33496,y:34223,varname:node_3288,prsc:2|A-9415-OUT,B-415-RGB,T-7736-A;n:type:ShaderForge.SFN_Tex2d,id:415,x:33038,y:34422,ptovrint:False,ptlb:Art Emission,ptin:_ArtEmission,varname:node_415,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:59d2d107650fb4f4a93e0962301e23e9,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Lerp,id:5673,x:33783,y:33936,varname:node_5673,prsc:2|A-5465-RGB,B-3910-OUT,T-6678-R;n:type:ShaderForge.SFN_Lerp,id:6412,x:33783,y:33628,varname:node_6412,prsc:2|A-9829-OUT,B-5349-OUT,T-6678-R;n:type:ShaderForge.SFN_Lerp,id:5055,x:33783,y:33773,varname:node_5055,prsc:2|A-4401-OUT,B-7025-OUT,T-6678-R;n:type:ShaderForge.SFN_Slider,id:9829,x:33438,y:33595,ptovrint:False,ptlb:Frame Metal,ptin:_FrameMetal,varname:node_9829,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:4401,x:33413,y:33718,ptovrint:False,ptlb:Frame Gloss,ptin:_FrameGloss,varname:node_4401,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7777783,max:1;n:type:ShaderForge.SFN_Lerp,id:7040,x:33783,y:34098,varname:node_7040,prsc:2|A-4654-OUT,B-3288-OUT,T-6678-R;n:type:ShaderForge.SFN_Vector1,id:4654,x:33496,y:34149,varname:node_4654,prsc:2,v1:0;n:type:ShaderForge.SFN_Get,id:6142,x:35451,y:32129,varname:node_6142,prsc:2|IN-144-OUT;n:type:ShaderForge.SFN_Lerp,id:8173,x:35685,y:32267,varname:node_8173,prsc:2|A-3413-OUT,B-8810-OUT,T-8220-VFACE;n:type:ShaderForge.SFN_Set,id:6048,x:34635,y:32489,varname:Gradation,prsc:2|IN-7484-OUT;n:type:ShaderForge.SFN_Get,id:9897,x:32807,y:34240,varname:node_9897,prsc:2|IN-6048-OUT;n:type:ShaderForge.SFN_Vector1,id:3413,x:35451,y:32280,varname:node_3413,prsc:2,v1:0;n:type:ShaderForge.SFN_Set,id:1583,x:34017,y:33451,varname:OmoteMasterA,prsc:2|IN-5783-OUT;n:type:ShaderForge.SFN_Set,id:8687,x:34002,y:33628,varname:OmoteMasterM,prsc:2|IN-6412-OUT;n:type:ShaderForge.SFN_Set,id:6763,x:34014,y:33792,varname:OmoteMasterG,prsc:2|IN-5055-OUT;n:type:ShaderForge.SFN_Set,id:5224,x:34029,y:33953,varname:OmoteMasterN,prsc:2|IN-5673-OUT;n:type:ShaderForge.SFN_Set,id:7592,x:34029,y:34107,varname:OmoteMasterE,prsc:2|IN-7040-OUT;n:type:ShaderForge.SFN_Get,id:8388,x:35213,y:31698,varname:node_8388,prsc:2|IN-1583-OUT;n:type:ShaderForge.SFN_Get,id:3802,x:34828,y:31709,varname:node_3802,prsc:2|IN-8687-OUT;n:type:ShaderForge.SFN_Get,id:802,x:34807,y:31955,varname:node_802,prsc:2|IN-6763-OUT;n:type:ShaderForge.SFN_Get,id:311,x:34959,y:32174,varname:node_311,prsc:2|IN-5224-OUT;n:type:ShaderForge.SFN_Get,id:8810,x:35451,y:32347,varname:node_8810,prsc:2|IN-7592-OUT;n:type:ShaderForge.SFN_Set,id:1953,x:34047,y:34765,varname:UraMasterA,prsc:2|IN-6329-OUT;n:type:ShaderForge.SFN_Set,id:154,x:33882,y:34845,varname:UraMasterMG,prsc:2|IN-9704-A;n:type:ShaderForge.SFN_Get,id:1906,x:35461,y:31670,varname:node_1906,prsc:2|IN-1953-OUT;n:type:ShaderForge.SFN_Get,id:6255,x:35451,y:31890,varname:node_6255,prsc:2|IN-154-OUT;n:type:ShaderForge.SFN_Add,id:837,x:35123,y:31769,varname:node_837,prsc:2|A-3802-OUT,B-6316-R;n:type:ShaderForge.SFN_Tex2d,id:6316,x:34818,y:31798,ptovrint:False,ptlb:Nisu Tex,ptin:_NisuTex,varname:node_6316,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6685679ca781c6540996f8eb1183a3f6,ntxv:2,isnm:False|UVIN-1582-UVOUT;n:type:ShaderForge.SFN_Add,id:2992,x:35123,y:31903,varname:node_2992,prsc:2|A-802-OUT,B-4440-OUT;n:type:ShaderForge.SFN_Tex2d,id:351,x:34925,y:32387,ptovrint:False,ptlb:Nisu Normal,ptin:_NisuNormal,varname:node_351,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5066e27fa10bfd441af17761d75b0e9b,ntxv:3,isnm:True;n:type:ShaderForge.SFN_NormalBlend,id:8803,x:35235,y:32178,varname:node_8803,prsc:2|BSE-311-OUT,DTL-874-OUT;n:type:ShaderForge.SFN_Lerp,id:874,x:35179,y:32425,varname:node_874,prsc:2|A-7785-OUT,B-351-RGB,T-7597-OUT;n:type:ShaderForge.SFN_Get,id:7785,x:34904,y:32300,varname:node_7785,prsc:2|IN-144-OUT;n:type:ShaderForge.SFN_Slider,id:7597,x:34847,y:32573,ptovrint:False,ptlb:Nisu Normal Power,ptin:_NisuNormalPower,varname:node_7597,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Rotator,id:9331,x:32572,y:32838,varname:node_9331,prsc:2|UVIN-96-UVOUT,ANG-6211-OUT;n:type:ShaderForge.SFN_Pi,id:6759,x:32123,y:33038,varname:node_6759,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6211,x:32277,y:32916,varname:node_6211,prsc:2|A-6341-OUT,B-6759-OUT;n:type:ShaderForge.SFN_Slider,id:6341,x:31958,y:32953,ptovrint:False,ptlb:Sub Kira Angle,ptin:_SubKiraAngle,varname:node_6341,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.4701528,max:2;n:type:ShaderForge.SFN_Multiply,id:4440,x:35123,y:32053,varname:node_4440,prsc:2|A-6316-R,B-8666-OUT;n:type:ShaderForge.SFN_Vector1,id:8666,x:34828,y:32070,varname:node_8666,prsc:2,v1:0.4;n:type:ShaderForge.SFN_LightVector,id:9157,x:32437,y:34313,varname:node_9157,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:4032,x:32437,y:34497,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:8616,x:32646,y:34389,varname:node_8616,prsc:2,dt:0|A-9157-OUT,B-4032-OUT;n:type:ShaderForge.SFN_Smoothstep,id:8553,x:32828,y:34314,varname:node_8553,prsc:2|A-5499-OUT,B-7045-OUT,V-8616-OUT;n:type:ShaderForge.SFN_Vector1,id:5499,x:32613,y:34210,varname:node_5499,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:7045,x:32598,y:34270,varname:node_7045,prsc:2,v1:1;n:type:ShaderForge.SFN_Lerp,id:3026,x:33025,y:34221,varname:node_3026,prsc:2|A-5499-OUT,B-9897-OUT,T-8553-OUT;n:type:ShaderForge.SFN_Multiply,id:8392,x:33595,y:33330,varname:node_8392,prsc:2|A-5675-RGB,B-4216-OUT;n:type:ShaderForge.SFN_Clamp01,id:5615,x:35320,y:31769,varname:node_5615,prsc:2|IN-837-OUT;n:type:ShaderForge.SFN_Clamp01,id:114,x:35320,y:31903,varname:node_114,prsc:2|IN-2992-OUT;n:type:ShaderForge.SFN_Set,id:9082,x:31319,y:32673,varname:SkyBox,prsc:2|IN-8652-RGB;n:type:ShaderForge.SFN_Get,id:4216,x:33351,y:33356,varname:node_4216,prsc:2|IN-9082-OUT;n:type:ShaderForge.SFN_Get,id:3728,x:35106,y:31429,varname:node_3728,prsc:2|IN-9082-OUT;n:type:ShaderForge.SFN_Multiply,id:1214,x:35365,y:31419,varname:node_1214,prsc:2|A-3728-OUT,B-6316-R;n:type:ShaderForge.SFN_Add,id:423,x:35555,y:31451,varname:node_423,prsc:2|A-1214-OUT,B-8388-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:4003,x:31288,y:32904,ptovrint:False,ptlb:Ambient Color,ptin:_AmbientColor,varname:node_4003,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-3041-OUT,B-6067-RGB;n:type:ShaderForge.SFN_Vector1,id:3041,x:31069,y:32833,varname:node_3041,prsc:2,v1:1;n:type:ShaderForge.SFN_Get,id:623,x:33203,y:33394,varname:node_623,prsc:2|IN-3858-OUT;n:type:ShaderForge.SFN_Set,id:3858,x:31479,y:32904,varname:AmbientColor,prsc:2|IN-4003-OUT;n:type:ShaderForge.SFN_Multiply,id:7457,x:33477,y:33446,varname:node_7457,prsc:2|A-623-OUT,B-94-OUT;n:type:ShaderForge.SFN_Multiply,id:9415,x:33249,y:34223,varname:node_9415,prsc:2|A-9368-OUT,B-3026-OUT;n:type:ShaderForge.SFN_Get,id:9368,x:33004,y:34165,varname:node_9368,prsc:2|IN-3858-OUT;n:type:ShaderForge.SFN_Multiply,id:6329,x:33882,y:34709,varname:node_6329,prsc:2|A-1956-OUT,B-9704-RGB;n:type:ShaderForge.SFN_Get,id:1956,x:33676,y:34685,varname:node_1956,prsc:2|IN-3858-OUT;n:type:ShaderForge.SFN_HalfVector,id:262,x:31711,y:31957,varname:node_262,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:4372,x:31659,y:33554,varname:node_4372,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:1372,x:31344,y:33832,varname:node_1372,prsc:2;n:type:ShaderForge.SFN_Rotator,id:2179,x:31906,y:33536,varname:node_2179,prsc:2|UVIN-4372-UVOUT,ANG-452-OUT,SPD-7254-OUT;n:type:ShaderForge.SFN_Multiply,id:7254,x:31871,y:33761,varname:node_7254,prsc:2|A-2570-OUT,B-480-OUT;n:type:ShaderForge.SFN_Slider,id:480,x:31115,y:34180,ptovrint:False,ptlb:node_480,ptin:_node_480,varname:node_480,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.192077,max:0.5;n:type:ShaderForge.SFN_Slider,id:4015,x:31159,y:33742,ptovrint:False,ptlb:node_4015,ptin:_node_4015,varname:node_4015,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:452,x:31611,y:33732,varname:node_452,prsc:2|A-4015-OUT,B-1372-TSL;n:type:ShaderForge.SFN_Slider,id:7699,x:32777,y:33333,ptovrint:False,ptlb:node_7699,ptin:_node_7699,varname:node_7699,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:100,max:100;n:type:ShaderForge.SFN_Divide,id:2570,x:31610,y:34083,varname:node_2570,prsc:2|A-1372-T,B-3164-TSL;n:type:ShaderForge.SFN_Time,id:3164,x:31344,y:33981,varname:node_3164,prsc:2;n:type:ShaderForge.SFN_Panner,id:2263,x:32079,y:33993,varname:node_2263,prsc:2,spu:1,spv:1|UVIN-2179-UVOUT,DIST-1372-T;n:type:ShaderForge.SFN_Time,id:3845,x:33877,y:31676,varname:node_3845,prsc:2;n:type:ShaderForge.SFN_Slider,id:6185,x:33682,y:31401,ptovrint:False,ptlb:node_4015_copy,ptin:_node_4015_copy,varname:_node_4015_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3.173913,max:5;n:type:ShaderForge.SFN_Multiply,id:1699,x:34022,y:31501,varname:node_1699,prsc:2|A-6185-OUT,B-3845-T;n:type:ShaderForge.SFN_Panner,id:1582,x:34238,y:31729,varname:node_1582,prsc:2,spu:1,spv:1|UVIN-6477-UVOUT,DIST-1699-OUT;n:type:ShaderForge.SFN_TexCoord,id:6477,x:34193,y:31269,varname:node_6477,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_TexCoord,id:9878,x:31913,y:33159,varname:node_9878,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:3567,x:31913,y:33316,varname:node_3567,prsc:2;n:type:ShaderForge.SFN_Rotator,id:9347,x:32115,y:33169,varname:node_9347,prsc:2|UVIN-9878-UVOUT,ANG-3567-TSL;n:type:ShaderForge.SFN_Panner,id:567,x:32142,y:33316,varname:node_567,prsc:2,spu:1,spv:1|UVIN-9347-UVOUT,DIST-3567-T;proporder:7736-762-2145-415-6678-5465-5675-9829-4401-8816-2657-7089-3278-2550-9841-5187-2407-7958-4835-6341-4785-7737-7141-5152-6316-351-7597-8652-9371-7484-8034-7507-9704-4003-480-4015-7699-6185;pass:END;sub:END;*/

Shader "nDxShasder/nDxKiraCardEX" {
    Properties {
        _MainTex ("Art Tex", 2D) = "white" {}
        _ArtMetal ("Art Metal", Range(0, 1)) = 1
        _ArtGloss ("Art Gloss", Range(0, 1)) = 0.2136752
        _ArtEmission ("Art Emission", 2D) = "black" {}
        _FrameTex ("Frame Tex", 2D) = "white" {}
        _FrameNormal ("Frame Normal", 2D) = "bump" {}
        _FrameColor ("Frame Color", Color) = (0.8970588,0.7652294,0.4815095,1)
        _FrameMetal ("Frame Metal", Range(0, 1)) = 1
        _FrameGloss ("Frame Gloss", Range(0, 1)) = 0.7777783
        _KiraTex ("Kira Tex", 2D) = "bump" {}
        [HDR]_KiraColor ("Kira Color", Color) = (1,1,1,1)
        _KiraPower ("Kira Power", Range(0, 1)) = 0
        _KiraTile ("Kira Tile", Range(0, 1)) = 0.5555556
        _KiraAngle ("Kira Angle", Range(0, 2)) = 0.2368918
        _KiraMetal ("Kira Metal", Range(0, 1)) = 1
        _KiraGloss ("Kira Gloss", Range(0, 1)) = 0.7264957
        _SubKiraTex ("Sub Kira Tex", 2D) = "bump" {}
        _SubKiraPower ("Sub Kira Power", Range(0, 1)) = 1
        _SubKiraTile ("Sub Kira Tile", Range(0, 1)) = 1
        _SubKiraAngle ("Sub Kira Angle", Range(0, 2)) = 0.4701528
        _HoloTex ("Holo Tex", 2D) = "white" {}
        _HoloShift ("Holo Shift", Range(0, 2)) = 2
        _HoloBrightness ("Holo Brightness", Range(0, 1)) = 0.5
        _CardDistortion ("Card Distortion", Range(0, 1)) = 0
        _NisuTex ("Nisu Tex", 2D) = "black" {}
        _NisuNormal ("Nisu Normal", 2D) = "bump" {}
        _NisuNormalPower ("Nisu Normal Power", Range(0, 1)) = 0
        _SkyBox ("SkyBox", Cube) = "_Skybox" {}
        _SkyBoxColor ("SkyBox Color", Color) = (0.5,0.5,0.5,1)
        [MaterialToggle] _Gradation ("Gradation", Float ) = 0.5
        _GradColor ("Grad Color", Color) = (0.5,0.5,0.5,1)
        _GradAngle ("Grad Angle", Range(0, 2)) = 0
        _BackFace ("BackFace", 2D) = "white" {}
        [MaterialToggle] _AmbientColor ("Ambient Color", Float ) = 1
        _node_480 ("node_480", Range(0, 0.5)) = 0.192077
        _node_4015 ("node_4015", Range(0, 1)) = 1
        _node_7699 ("node_7699", Range(0, 100)) = 100
        _node_4015_copy ("node_4015_copy", Range(0, 5)) = 3.173913
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _FrameNormal; uniform float4 _FrameNormal_ST;
            uniform sampler2D _FrameTex; uniform float4 _FrameTex_ST;
            uniform sampler2D _BackFace; uniform float4 _BackFace_ST;
            uniform sampler2D _SubKiraTex; uniform float4 _SubKiraTex_ST;
            uniform samplerCUBE _SkyBox;
            uniform sampler2D _HoloTex; uniform float4 _HoloTex_ST;
            uniform sampler2D _KiraTex; uniform float4 _KiraTex_ST;
            uniform sampler2D _ArtEmission; uniform float4 _ArtEmission_ST;
            uniform sampler2D _NisuTex; uniform float4 _NisuTex_ST;
            uniform sampler2D _NisuNormal; uniform float4 _NisuNormal_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, _FrameColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraTile)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraAngle)
                UNITY_DEFINE_INSTANCED_PROP( float, _SubKiraTile)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraMetal)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraGloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraPower)
                UNITY_DEFINE_INSTANCED_PROP( float, _HoloShift)
                UNITY_DEFINE_INSTANCED_PROP( float, _HoloBrightness)
                UNITY_DEFINE_INSTANCED_PROP( float4, _KiraColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _CardDistortion)
                UNITY_DEFINE_INSTANCED_PROP( float, _SubKiraPower)
                UNITY_DEFINE_INSTANCED_PROP( float4, _SkyBoxColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _GradAngle)
                UNITY_DEFINE_INSTANCED_PROP( float4, _GradColor)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _Gradation)
                UNITY_DEFINE_INSTANCED_PROP( float, _ArtMetal)
                UNITY_DEFINE_INSTANCED_PROP( float, _ArtGloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _FrameMetal)
                UNITY_DEFINE_INSTANCED_PROP( float, _FrameGloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _NisuNormalPower)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _AmbientColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _node_480)
                UNITY_DEFINE_INSTANCED_PROP( float, _node_4015)
                UNITY_DEFINE_INSTANCED_PROP( float, _node_4015_copy)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 PlaneNormal = float3(0,0,1);
                float3 _FrameNormal_var = UnpackNormal(tex2D(_FrameNormal,TRANSFORM_TEX(i.uv0, _FrameNormal)));
                float3 node_5014 = PlaneNormal;
                float _KiraTile_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraTile );
                float node_2093 = 1.0;
                float2 node_1243_tc_rcp = float2(1.0,1.0)/float2( _KiraTile_var, _KiraTile_var );
                float node_1243_ty = floor(node_2093 * node_1243_tc_rcp.x);
                float node_1243_tx = node_2093 - _KiraTile_var * node_1243_ty;
                float _KiraAngle_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraAngle );
                float node_9774_ang = (_KiraAngle_var*3.141592654);
                float node_9774_spd = 1.0;
                float node_9774_cos = cos(node_9774_spd*node_9774_ang);
                float node_9774_sin = sin(node_9774_spd*node_9774_ang);
                float2 node_9774_piv = float2(0.5,0.5);
                float2 node_9774 = (mul(i.uv0-node_9774_piv,float2x2( node_9774_cos, -node_9774_sin, node_9774_sin, node_9774_cos))+node_9774_piv);
                float2 node_1243 = (node_9774 + float2(node_1243_tx, node_1243_ty)) * node_1243_tc_rcp;
                float3 node_9884 = UnpackNormal(tex2D(_KiraTex,TRANSFORM_TEX(node_1243, _KiraTex)));
                float _KiraPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraPower );
                float _SubKiraTile_var = UNITY_ACCESS_INSTANCED_PROP( Props, _SubKiraTile );
                float2 node_6548_tc_rcp = float2(1.0,1.0)/float2( _SubKiraTile_var, _SubKiraTile_var );
                float node_6548_ty = floor(node_2093 * node_6548_tc_rcp.x);
                float node_6548_tx = node_2093 - _SubKiraTile_var * node_6548_ty;
                float4 node_1372 = _Time;
                float _node_4015_var = UNITY_ACCESS_INSTANCED_PROP( Props, _node_4015 );
                float4 node_3164 = _Time;
                float _node_480_var = UNITY_ACCESS_INSTANCED_PROP( Props, _node_480 );
                float node_2179_ang = (_node_4015_var*node_1372.r);
                float node_2179_spd = ((node_1372.g/node_3164.r)*_node_480_var);
                float node_2179_cos = cos(node_2179_spd*node_2179_ang);
                float node_2179_sin = sin(node_2179_spd*node_2179_ang);
                float2 node_2179_piv = float2(0.5,0.5);
                float2 node_2179 = (mul(i.uv0-node_2179_piv,float2x2( node_2179_cos, -node_2179_sin, node_2179_sin, node_2179_cos))+node_2179_piv);
                float2 node_6548 = ((node_2179+node_1372.g*float2(1,1)) + float2(node_6548_tx, node_6548_ty)) * node_6548_tc_rcp;
                float3 _SubKiraTex_var = UnpackNormal(tex2D(_SubKiraTex,TRANSFORM_TEX(node_6548, _SubKiraTex)));
                float _SubKiraPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _SubKiraPower );
                float3 node_3827_nrm_base = lerp(node_5014,saturate(max(node_9884.rgb,node_9884.rgb)),_KiraPower_var) + float3(0,0,1);
                float3 node_3827_nrm_detail = lerp(node_5014,_SubKiraTex_var.rgb,_SubKiraPower_var) * float3(-1,-1,1);
                float3 node_3827_nrm_combined = node_3827_nrm_base*dot(node_3827_nrm_base, node_3827_nrm_detail)/node_3827_nrm_base.z - node_3827_nrm_detail;
                float3 node_3827 = node_3827_nrm_combined;
                float3 KiraNormal = node_3827;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _FrameTex_var = tex2D(_FrameTex,TRANSFORM_TEX(i.uv0, _FrameTex));
                float3 OmoteMasterN = lerp(_FrameNormal_var.rgb,lerp(KiraNormal,PlaneNormal,_MainTex_var.a),_FrameTex_var.r);
                float3 _NisuNormal_var = UnpackNormal(tex2D(_NisuNormal,TRANSFORM_TEX(i.uv0, _NisuNormal)));
                float _NisuNormalPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _NisuNormalPower );
                float3 node_8803_nrm_base = OmoteMasterN + float3(0,0,1);
                float3 node_8803_nrm_detail = lerp(PlaneNormal,_NisuNormal_var.rgb,_NisuNormalPower_var) * float3(-1,-1,1);
                float3 node_8803_nrm_combined = node_8803_nrm_base*dot(node_8803_nrm_base, node_8803_nrm_detail)/node_8803_nrm_base.z - node_8803_nrm_detail;
                float3 node_8803 = node_8803_nrm_combined;
                float3 normalLocal = lerp(PlaneNormal,node_8803,isFrontFace);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float2 node_1766 = float2((1.0 - i.uv0.r),i.uv0.g.r);
                float4 _BackFace_var = tex2D(_BackFace,TRANSFORM_TEX(node_1766, _BackFace));
                float UraMasterMG = _BackFace_var.a;
                float node_6255 = UraMasterMG;
                float _FrameGloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _FrameGloss );
                float _KiraGloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraGloss );
                float _ArtGloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ArtGloss );
                float OmoteMasterG = lerp(_FrameGloss_var,lerp(_KiraGloss_var,_ArtGloss_var,_MainTex_var.a),_FrameTex_var.r);
                float _node_4015_copy_var = UNITY_ACCESS_INSTANCED_PROP( Props, _node_4015_copy );
                float4 node_3845 = _Time;
                float2 node_1582 = (i.uv0+(_node_4015_copy_var*node_3845.g)*float2(1,1));
                float4 _NisuTex_var = tex2D(_NisuTex,TRANSFORM_TEX(node_1582, _NisuTex));
                float gloss = lerp(node_6255,saturate((OmoteMasterG+(_NisuTex_var.r*0.4))),isFrontFace);
                float perceptualRoughness = 1.0 - lerp(node_6255,saturate((OmoteMasterG+(_NisuTex_var.r*0.4))),isFrontFace);
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float _FrameMetal_var = UNITY_ACCESS_INSTANCED_PROP( Props, _FrameMetal );
                float _KiraMetal_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraMetal );
                float _ArtMetal_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ArtMetal );
                float OmoteMasterM = lerp(_FrameMetal_var,lerp(_KiraMetal_var,_ArtMetal_var,_MainTex_var.a),_FrameTex_var.r);
                float3 specularColor = lerp(node_6255,saturate((OmoteMasterM+_NisuTex_var.r)),isFrontFace);
                float specularMonochrome;
                float3 _AmbientColor_var = lerp( 1.0, UNITY_LIGHTMODEL_AMBIENT.rgb, UNITY_ACCESS_INSTANCED_PROP( Props, _AmbientColor ) );
                float3 AmbientColor = _AmbientColor_var;
                float3 UraMasterA = (AmbientColor*_BackFace_var.rgb);
                float3 SkyBox = texCUBE(_SkyBox,viewReflectDirection).rgb;
                float4 _FrameColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _FrameColor );
                float node_2120 = dot(halfDirection,i.normalDir);
                float _HoloShift_var = UNITY_ACCESS_INSTANCED_PROP( Props, _HoloShift );
                float2 node_1814 = (float2(node_2120,node_2120)*_HoloShift_var);
                float4 node_4446 = tex2D(_HoloTex,TRANSFORM_TEX(node_1814, _HoloTex));
                float3 node_9398_nrm_base = node_4446.rgb + float3(0,0,1);
                float3 node_9398_nrm_detail = KiraNormal * float3(-1,-1,1);
                float3 node_9398_nrm_combined = node_9398_nrm_base*dot(node_9398_nrm_base, node_9398_nrm_detail)/node_9398_nrm_base.z - node_9398_nrm_detail;
                float3 node_9398 = node_9398_nrm_combined;
                float3 node_8865 = UnpackNormal(tex2D(_KiraTex,TRANSFORM_TEX(node_1243, _KiraTex)));
                float _CardDistortion_var = UNITY_ACCESS_INSTANCED_PROP( Props, _CardDistortion );
                float3 node_7952_nrm_base = node_9398 + float3(0,0,1);
                float3 node_7952_nrm_detail = lerp(PlaneNormal,node_8865.rgb,_CardDistortion_var) * float3(-1,-1,1);
                float3 node_7952_nrm_combined = node_7952_nrm_base*dot(node_7952_nrm_base, node_7952_nrm_detail)/node_7952_nrm_base.z - node_7952_nrm_detail;
                float3 node_7952 = node_7952_nrm_combined;
                float2 node_2449 = mul( UNITY_MATRIX_V, float4(node_7952,0) ).xyz.rgb.rg;
                float4 node_3136 = tex2D(_HoloTex,TRANSFORM_TEX(node_2449, _HoloTex));
                float _HoloBrightness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _HoloBrightness );
                float3 node_5401 = saturate(( _HoloBrightness_var > 0.5 ? (1.0-(1.0-2.0*(_HoloBrightness_var-0.5))*(1.0-node_3136.rgb)) : (2.0*_HoloBrightness_var*node_3136.rgb) )).rgb;
                float4 _KiraColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraColor );
                float3 KiraAlbedo = ((node_5401+node_5401.b)*_KiraColor_var.rgb);
                float3 OmoteMasterA = lerp((_FrameColor_var.rgb*SkyBox),(AmbientColor*lerp(KiraAlbedo,_MainTex_var.rgb,_MainTex_var.a)),_FrameTex_var.r);
                float3 diffuseColor = lerp(UraMasterA,((SkyBox*_NisuTex_var.r)+OmoteMasterA),isFrontFace); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
////// Emissive:
                float node_3413 = 0.0;
                float node_4654 = 0.0;
                float node_5499 = 0.0;
                float4 _SkyBoxColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _SkyBoxColor );
                float4 _GradColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GradColor );
                float _GradAngle_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GradAngle );
                float node_1452_ang = (_GradAngle_var*3.141592654);
                float node_1452_spd = 1.0;
                float node_1452_cos = cos(node_1452_spd*node_1452_ang);
                float node_1452_sin = sin(node_1452_spd*node_1452_ang);
                float2 node_1452_piv = float2(0.5,0.5);
                float2 node_1452 = (mul(i.uv0-node_1452_piv,float2x2( node_1452_cos, -node_1452_sin, node_1452_sin, node_1452_cos))+node_1452_piv);
                float3 _Gradation_var = lerp( _SkyBoxColor_var.rgb, lerp(_SkyBoxColor_var.rgb,_GradColor_var.rgb,node_1452.r), UNITY_ACCESS_INSTANCED_PROP( Props, _Gradation ) );
                float3 Gradation = _Gradation_var;
                float4 _ArtEmission_var = tex2D(_ArtEmission,TRANSFORM_TEX(i.uv0, _ArtEmission));
                float3 OmoteMasterE = lerp(float3(node_4654,node_4654,node_4654),lerp((AmbientColor*lerp(float3(node_5499,node_5499,node_5499),Gradation,smoothstep( node_5499, 1.0, dot(lightDirection,i.normalDir) ))),_ArtEmission_var.rgb,_MainTex_var.a),_FrameTex_var.r);
                float3 emissive = lerp(float3(node_3413,node_3413,node_3413),OmoteMasterE,isFrontFace);
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _FrameNormal; uniform float4 _FrameNormal_ST;
            uniform sampler2D _FrameTex; uniform float4 _FrameTex_ST;
            uniform sampler2D _BackFace; uniform float4 _BackFace_ST;
            uniform sampler2D _SubKiraTex; uniform float4 _SubKiraTex_ST;
            uniform samplerCUBE _SkyBox;
            uniform sampler2D _HoloTex; uniform float4 _HoloTex_ST;
            uniform sampler2D _KiraTex; uniform float4 _KiraTex_ST;
            uniform sampler2D _ArtEmission; uniform float4 _ArtEmission_ST;
            uniform sampler2D _NisuTex; uniform float4 _NisuTex_ST;
            uniform sampler2D _NisuNormal; uniform float4 _NisuNormal_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, _FrameColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraTile)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraAngle)
                UNITY_DEFINE_INSTANCED_PROP( float, _SubKiraTile)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraMetal)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraGloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _KiraPower)
                UNITY_DEFINE_INSTANCED_PROP( float, _HoloShift)
                UNITY_DEFINE_INSTANCED_PROP( float, _HoloBrightness)
                UNITY_DEFINE_INSTANCED_PROP( float4, _KiraColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _CardDistortion)
                UNITY_DEFINE_INSTANCED_PROP( float, _SubKiraPower)
                UNITY_DEFINE_INSTANCED_PROP( float4, _SkyBoxColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _GradAngle)
                UNITY_DEFINE_INSTANCED_PROP( float4, _GradColor)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _Gradation)
                UNITY_DEFINE_INSTANCED_PROP( float, _ArtMetal)
                UNITY_DEFINE_INSTANCED_PROP( float, _ArtGloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _FrameMetal)
                UNITY_DEFINE_INSTANCED_PROP( float, _FrameGloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _NisuNormalPower)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _AmbientColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _node_480)
                UNITY_DEFINE_INSTANCED_PROP( float, _node_4015)
                UNITY_DEFINE_INSTANCED_PROP( float, _node_4015_copy)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 PlaneNormal = float3(0,0,1);
                float3 _FrameNormal_var = UnpackNormal(tex2D(_FrameNormal,TRANSFORM_TEX(i.uv0, _FrameNormal)));
                float3 node_5014 = PlaneNormal;
                float _KiraTile_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraTile );
                float node_2093 = 1.0;
                float2 node_1243_tc_rcp = float2(1.0,1.0)/float2( _KiraTile_var, _KiraTile_var );
                float node_1243_ty = floor(node_2093 * node_1243_tc_rcp.x);
                float node_1243_tx = node_2093 - _KiraTile_var * node_1243_ty;
                float _KiraAngle_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraAngle );
                float node_9774_ang = (_KiraAngle_var*3.141592654);
                float node_9774_spd = 1.0;
                float node_9774_cos = cos(node_9774_spd*node_9774_ang);
                float node_9774_sin = sin(node_9774_spd*node_9774_ang);
                float2 node_9774_piv = float2(0.5,0.5);
                float2 node_9774 = (mul(i.uv0-node_9774_piv,float2x2( node_9774_cos, -node_9774_sin, node_9774_sin, node_9774_cos))+node_9774_piv);
                float2 node_1243 = (node_9774 + float2(node_1243_tx, node_1243_ty)) * node_1243_tc_rcp;
                float3 node_9884 = UnpackNormal(tex2D(_KiraTex,TRANSFORM_TEX(node_1243, _KiraTex)));
                float _KiraPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraPower );
                float _SubKiraTile_var = UNITY_ACCESS_INSTANCED_PROP( Props, _SubKiraTile );
                float2 node_6548_tc_rcp = float2(1.0,1.0)/float2( _SubKiraTile_var, _SubKiraTile_var );
                float node_6548_ty = floor(node_2093 * node_6548_tc_rcp.x);
                float node_6548_tx = node_2093 - _SubKiraTile_var * node_6548_ty;
                float4 node_1372 = _Time;
                float _node_4015_var = UNITY_ACCESS_INSTANCED_PROP( Props, _node_4015 );
                float4 node_3164 = _Time;
                float _node_480_var = UNITY_ACCESS_INSTANCED_PROP( Props, _node_480 );
                float node_2179_ang = (_node_4015_var*node_1372.r);
                float node_2179_spd = ((node_1372.g/node_3164.r)*_node_480_var);
                float node_2179_cos = cos(node_2179_spd*node_2179_ang);
                float node_2179_sin = sin(node_2179_spd*node_2179_ang);
                float2 node_2179_piv = float2(0.5,0.5);
                float2 node_2179 = (mul(i.uv0-node_2179_piv,float2x2( node_2179_cos, -node_2179_sin, node_2179_sin, node_2179_cos))+node_2179_piv);
                float2 node_6548 = ((node_2179+node_1372.g*float2(1,1)) + float2(node_6548_tx, node_6548_ty)) * node_6548_tc_rcp;
                float3 _SubKiraTex_var = UnpackNormal(tex2D(_SubKiraTex,TRANSFORM_TEX(node_6548, _SubKiraTex)));
                float _SubKiraPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _SubKiraPower );
                float3 node_3827_nrm_base = lerp(node_5014,saturate(max(node_9884.rgb,node_9884.rgb)),_KiraPower_var) + float3(0,0,1);
                float3 node_3827_nrm_detail = lerp(node_5014,_SubKiraTex_var.rgb,_SubKiraPower_var) * float3(-1,-1,1);
                float3 node_3827_nrm_combined = node_3827_nrm_base*dot(node_3827_nrm_base, node_3827_nrm_detail)/node_3827_nrm_base.z - node_3827_nrm_detail;
                float3 node_3827 = node_3827_nrm_combined;
                float3 KiraNormal = node_3827;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _FrameTex_var = tex2D(_FrameTex,TRANSFORM_TEX(i.uv0, _FrameTex));
                float3 OmoteMasterN = lerp(_FrameNormal_var.rgb,lerp(KiraNormal,PlaneNormal,_MainTex_var.a),_FrameTex_var.r);
                float3 _NisuNormal_var = UnpackNormal(tex2D(_NisuNormal,TRANSFORM_TEX(i.uv0, _NisuNormal)));
                float _NisuNormalPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _NisuNormalPower );
                float3 node_8803_nrm_base = OmoteMasterN + float3(0,0,1);
                float3 node_8803_nrm_detail = lerp(PlaneNormal,_NisuNormal_var.rgb,_NisuNormalPower_var) * float3(-1,-1,1);
                float3 node_8803_nrm_combined = node_8803_nrm_base*dot(node_8803_nrm_base, node_8803_nrm_detail)/node_8803_nrm_base.z - node_8803_nrm_detail;
                float3 node_8803 = node_8803_nrm_combined;
                float3 normalLocal = lerp(PlaneNormal,node_8803,isFrontFace);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float2 node_1766 = float2((1.0 - i.uv0.r),i.uv0.g.r);
                float4 _BackFace_var = tex2D(_BackFace,TRANSFORM_TEX(node_1766, _BackFace));
                float UraMasterMG = _BackFace_var.a;
                float node_6255 = UraMasterMG;
                float _FrameGloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _FrameGloss );
                float _KiraGloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraGloss );
                float _ArtGloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ArtGloss );
                float OmoteMasterG = lerp(_FrameGloss_var,lerp(_KiraGloss_var,_ArtGloss_var,_MainTex_var.a),_FrameTex_var.r);
                float _node_4015_copy_var = UNITY_ACCESS_INSTANCED_PROP( Props, _node_4015_copy );
                float4 node_3845 = _Time;
                float2 node_1582 = (i.uv0+(_node_4015_copy_var*node_3845.g)*float2(1,1));
                float4 _NisuTex_var = tex2D(_NisuTex,TRANSFORM_TEX(node_1582, _NisuTex));
                float gloss = lerp(node_6255,saturate((OmoteMasterG+(_NisuTex_var.r*0.4))),isFrontFace);
                float perceptualRoughness = 1.0 - lerp(node_6255,saturate((OmoteMasterG+(_NisuTex_var.r*0.4))),isFrontFace);
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float _FrameMetal_var = UNITY_ACCESS_INSTANCED_PROP( Props, _FrameMetal );
                float _KiraMetal_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraMetal );
                float _ArtMetal_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ArtMetal );
                float OmoteMasterM = lerp(_FrameMetal_var,lerp(_KiraMetal_var,_ArtMetal_var,_MainTex_var.a),_FrameTex_var.r);
                float3 specularColor = lerp(node_6255,saturate((OmoteMasterM+_NisuTex_var.r)),isFrontFace);
                float specularMonochrome;
                float3 _AmbientColor_var = lerp( 1.0, UNITY_LIGHTMODEL_AMBIENT.rgb, UNITY_ACCESS_INSTANCED_PROP( Props, _AmbientColor ) );
                float3 AmbientColor = _AmbientColor_var;
                float3 UraMasterA = (AmbientColor*_BackFace_var.rgb);
                float3 SkyBox = texCUBE(_SkyBox,viewReflectDirection).rgb;
                float4 _FrameColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _FrameColor );
                float node_2120 = dot(halfDirection,i.normalDir);
                float _HoloShift_var = UNITY_ACCESS_INSTANCED_PROP( Props, _HoloShift );
                float2 node_1814 = (float2(node_2120,node_2120)*_HoloShift_var);
                float4 node_4446 = tex2D(_HoloTex,TRANSFORM_TEX(node_1814, _HoloTex));
                float3 node_9398_nrm_base = node_4446.rgb + float3(0,0,1);
                float3 node_9398_nrm_detail = KiraNormal * float3(-1,-1,1);
                float3 node_9398_nrm_combined = node_9398_nrm_base*dot(node_9398_nrm_base, node_9398_nrm_detail)/node_9398_nrm_base.z - node_9398_nrm_detail;
                float3 node_9398 = node_9398_nrm_combined;
                float3 node_8865 = UnpackNormal(tex2D(_KiraTex,TRANSFORM_TEX(node_1243, _KiraTex)));
                float _CardDistortion_var = UNITY_ACCESS_INSTANCED_PROP( Props, _CardDistortion );
                float3 node_7952_nrm_base = node_9398 + float3(0,0,1);
                float3 node_7952_nrm_detail = lerp(PlaneNormal,node_8865.rgb,_CardDistortion_var) * float3(-1,-1,1);
                float3 node_7952_nrm_combined = node_7952_nrm_base*dot(node_7952_nrm_base, node_7952_nrm_detail)/node_7952_nrm_base.z - node_7952_nrm_detail;
                float3 node_7952 = node_7952_nrm_combined;
                float2 node_2449 = mul( UNITY_MATRIX_V, float4(node_7952,0) ).xyz.rgb.rg;
                float4 node_3136 = tex2D(_HoloTex,TRANSFORM_TEX(node_2449, _HoloTex));
                float _HoloBrightness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _HoloBrightness );
                float3 node_5401 = saturate(( _HoloBrightness_var > 0.5 ? (1.0-(1.0-2.0*(_HoloBrightness_var-0.5))*(1.0-node_3136.rgb)) : (2.0*_HoloBrightness_var*node_3136.rgb) )).rgb;
                float4 _KiraColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _KiraColor );
                float3 KiraAlbedo = ((node_5401+node_5401.b)*_KiraColor_var.rgb);
                float3 OmoteMasterA = lerp((_FrameColor_var.rgb*SkyBox),(AmbientColor*lerp(KiraAlbedo,_MainTex_var.rgb,_MainTex_var.a)),_FrameTex_var.r);
                float3 diffuseColor = lerp(UraMasterA,((SkyBox*_NisuTex_var.r)+OmoteMasterA),isFrontFace); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
