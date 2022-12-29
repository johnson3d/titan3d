macroScript EpicTools_PivotPainter category:"Epic Tools" buttontext:"Pivot Painter" tooltip:"Pivot Painter"
(
	
	escapeEnable=true
	
	/************************************************************************************************

	Pivot Painter version 1.0
	Written by Jonathan Lindquist at Epic Games
	
	*************************************************************************************************/

	/**************************************MENU CONTROLS****************************************/
		--To expand a tab by default change the appropriate variable to true. To collapse a tab enter false.
		global bPrepTools=true
		global bVertexPainter=false
		global bIndividual =	false
		global bHelp=false
	/**************************************MENU CONTROLS****************************************/
	
	
	
	-------------------------------------------------------- GUI settings
	
	global guiW=200
	global totalHeight=0 	
	global PrepToolsH = 337
	global vertexPainterH = 400
	global HelpH =530
	global IndividualH= 515
	global collapsedHeight= 22
	global floaterWidth=240
	global foliage_floater
	
	-------------------------------------------------------- object tracking 
	
	global LeafArray =#()
	global currBranchObj=undefined
	global currSplineObj =undefined
	global meshPivotObj=undefined
	global selectionSetInt=undefined
	global bContinue = true 
	
	global prepLeaf=undefined
	global bprepLeafMessage=true 
	global prepBranch=undefined
	global bprepBranchMessage= true 
	global selectionSetIntPrep=0
	global selListfound=false
	global branchfound=false
	
	struct customSelGroup (Branch,selSetName)
	global customSelectionGroupsArray
	global prefabListDropDownItemArray
	global customSelectionGroupsArray=#()
	global prefabListDropDownItemArray=#()
	global boxToLeaveSelSetArray=#()
	global boxArrays=#() 
	global alteredSelectionSets=#()
	global bPaintBranch=true
	------------------------------------------------------------------------------------------------------------------------Warning messages 
	global bBranchMessage  = false--changed
	global bSplineMessage  = false--changed
	global zSign=1.0
	global bLeafMessage = false--changed
	global branchMessage  =  "An editable poly parent mesh. \n"
	global splineMessage  = "A spline with at least 2 knots to use as a guide for your mesh pivots. \n"
	global leafMessage =  "A valid selection set containing at least one editable poly without any modifiers. \n"
	 
	global prepBranchMessage = "A valid branch editable poly object without any modifiers. \n"
	global prepLeafMessage = "A valid child editable poly object without any modifiers. \n"
	global prepMeshPivotObjMessage = "A valid editable polygon object with at least 1 vertex. \n"
	global combinedWarningMessage =""
	global combinedWarningMessagePrep =""

	global bLeafSelSetPrepMessage =false
	global lowestNumber=9999999999999999
	global splineVertArray= #()
	global individualVertArray = #()
	global finalDistanceArray = #()
	global fullVertArray=#()
	struct objPiv (obj, objVert, spln, splnSubline, splnVert, splnVertPos, vertDist)
	global finalMeshVertPairArray=#()
	global sortingArray =#()
	global dummyArray=#()
	global finalDummyDataArray= #()
	global uniqueDummyPositions=#();
	global doNotAddList=#();
	global leafWingFalloffPower=10
	global leafLengthFalloffPower=1
	global leafLengthFalloff=.5
	global leafWingFalloffY=0.0
	global leafWingFalloffX=0.0
	global leafWingFalloffPowerZ=10.0
	global leafWingFalloffZ =0.0
	global leafWingFalloffPowerY=10	
	global leafWingFalloffPowerX=10
	global shaderWSMultiplier=4096
	global bRunningPivotCheck=true 
	global expansionNumber =1 
	global processAllMeshNormals= true
	global acceptSlowness=false 
	global facelist 
	global vertlist
	global Nv =[0,0,0]
	global faceCount=0 
	global newAxis
	global selectionsetsArray=#()
	global halfButtonStringlength=15
	global fullButtonStringlength=30
	global selectionSetIntIndividual=0;
---------------------------------------------------------------------------------------------------------------------------------Progress Bar
	global createNewLeafPivot
	global PrepTools_rollout
	global pbProgress 
	global updateProgBar
	global pbNumberOfStages = 3
	global pbMaxOperations = 1000000.0
	global progBarAmount = 0.0
	global PaintIndividualPivots_rollout
	global pbIndividualPivots
	global pbCol = black as point3; 
	global PaintLeavesAndTrees_rollout
	global pbTreeVertexPainter
----------------------------------------------------------------------------------------------------------------------------End Progress Bar


	fn fixUVNames polyToFix = (
		for i = 1 to (polyop.getNumMaps polyToFix) do (ChannelInfo.NameChannel polyToFix 3 i ("UVChannel_" + i as string))
	)

	

	global dropList=#()
	fn updateDropDownList = (
		deleteSelectionSetList=#()
		dropList=#()
		if selectionSets.count > 0 do (
			for i in selectionSets do (
				if i.count == 0 then (
					append deleteSelectionSetList (i.name)
				) else (
					append dropList i.name
				)
			)
			if deleteSelectionSetList.count > 0 do (
				for i=1 to deleteSelectionSetList.count do (
					dItem=(deleteSelectionSetList[i] as string)
					deleteitem selectionSets dItem
				)
			)
		)
	)
		--for i in selectionSets do if i.count == 0 then deleteitem selectionSets i.name 
	
	 updateDropDownList();
	
	global truncateString		
	fn truncateString initialString NumberOfCharacters =(
			if initialString.count > (NumberOfCharacters) then (
				returnString=replace initialString (NumberOfCharacters) ((initialString.count-NumberOfCharacters)+1) "..."
				returnString
			) else (initialString)
	)		
			
	selectBranchDetachBtntext="Pick Selection Obj"
	selectLeafDetachBtntext="Pick Model to Proc"

	fn displayAutoErrorMessage = (
		format "*** % ***\n" (getCurrentException())
	)
	
	fn clamp num cMin cMax = (
		/*
			REWRITTEN - Chris Wood
			-if less than minimum, set to minimum
			-if more than maximum, set to maximum
		*/
		result = num
		if result < cMin
		then result = cMin
		else (
			if result > cMax
			then result = cMax
		)
		return result
	)
	
	
	fn adjustRolloutHeight = (	  
		totalHeight=0
		if bPrepTools == true  then totalHeight+=PrepToolsH else totalHeight+=collapsedHeight
		if bVertexPainter == true  then totalHeight+=vertexPainterH else totalHeight+=collapsedHeight
		if bHelp == true  then totalHeight+=HelpH else totalHeight+=collapsedHeight
		if bIndividual == true  then totalHeight+=IndividualH else totalHeight+=collapsedHeight		
		foliage_floater.size = [floaterWidth,totalHeight]
	)	  
	
	fn checkGeo geo =  isvalidobj geo and ClassOf geo == Editable_Poly and (getNumVerts geo > 0 ) and geo.modifiers.count == 0

	fn checkGeoDetachPen geo =  isvalidobj geo and ClassOf geo == Editable_Poly and geo != prepLeaf and geo != prepBranch 

	fn errorMessagePrep = (
		combinedWarningMessagePrep="Please select the following items:  \n\n" 
		if bprepBranchMessage == true then combinedWarningMessagePrep += prepBranchMessage 
		if bprepLeafMessage == true then combinedWarningMessagePrep += prepLeafMessage
		messagebox combinedWarningMessagePrep 
	)
	
	fn errorMessage = (
		combinedWarningMessage="Please select the following items:  \n\n" 
		if bBranchMessage == true then combinedWarningMessage += branchMessage 
		if bSplineMessage == true then combinedWarningMessage += splineMessage
		if bLeafMessage == true then combinedWarningMessage += leafMessage
		messagebox combinedWarningMessage 
	)
	global AlignPivotTo
	fn AlignPivotTo Obj Trgt=
	(		
		/*
			REWRITTEN - Chris Wood
			- check param types - Obj must be a node - Trgt must be a node or Matrix3
			- calc inverse of the target matrix for corrections
			- object's transform is to be changed to the target matrix
			- can correct the local (node to parent node) TM of children...
			- correction is to transform into world and back to the new target space giving the new local TM and applying this to the controllers
			- loop through child nodes and apply correction
			- can correct the object offset of this node...
			- correction is to transform into world and back to the new target space giving the new object offset and apply this to the offset components
		*/
		
		assert (IsValidNode Obj) message: "AlignPivotTo expected Obj to be a valid Node"
		targetTM = Trgt
		if (IsValidNode Trgt) then targetTM = Trgt.transform
		assert (ClassOf targetTM == Matrix3) message: "AlignPivotTo expected Trgt to be a valid Node or Matrix3" 
		
		invTargetTM = inverse targetTM
		
		for c in Obj.children
		do (
			newChildLocalTM = c.transform * invTargetTM
			c.scale.controller.value = newChildLocalTM.scalepart
			c.rotation.controller.value = newChildLocalTM.rotationpart
			c.pos.controller.value = newChildLocalTM.translationpart
		)			
		
		newObjOffsetTM = Obj.objectTransform * invTargetTM
		Obj.transform = targetTM
		Obj.objectOffsetScale = newObjOffsetTM.scalepart
		Obj.objectOffsetRot = newObjOffsetTM.rotationpart
		Obj.objectOffsetPos = newObjOffsetTM.translationpart		
	)
	fn findObjectAxis vertexID:1 numberOfExpansions:1 processAllNormals:false leaf:undefined =	(
		try with undo off(
			faceCount=0
			vertlist=vertexID
			if processAllNormals==true then (
				vertlist =- #{(polyop.getNumVerts leaf)+1}
				facelist = polyop.getFacesUsingVert leaf vertlist
			) else (
				for i=1 to numberOfExpansions do (
					facelist = polyop.getFacesUsingVert leaf vertlist
					vertlist = polyop.getVertsUsingFace leaf facelist
				)
			)
			count=0
			Nv =[0,0,0]
			vertexPivotPos=polyop.getVert leaf vertexID
			for i in facelist do (
				count+=1
				if i == 1 then (Nv = polyop.getFaceNormal leaf i ) 
				else (Nv += polyop.getFaceNormal leaf i )
			)
			faceCount = facelist.numberSet
			Nv = normalize (Nv/faceCount)
			Rv = normalize (vertexPivotPos-leaf.pos)
			Uv = normalize (cross  Nv Rv)
			Nv = -normalize (cross Uv Rv)
			newAxis = matrix3 -Rv -Uv  Nv  vertexPivotPos
		) 
		catch(
			displayAutoErrorMessage()
		)
	)
				
	fn convertWPtoUV Pos:[0,0,0] = (
		global temp=(Pos/shaderWSMultiplier)*[128,128,255]+[128,128,0]
		global tempx= clamp (temp[1]) 0 255
		global tempy= clamp (temp[2]) 0 255
		global tempz= clamp (temp[3]) 0 255
		global temp=[tempx,tempy,tempz]
	)

	fn resetObjectDisplay obj:undefined= (
			obj.showVertexColors = off
			obj.vertexColorsShaded = on
	)
	
	global fnDetachElementsInterpenetratingBtn
	fn fnDetachElementsInterpenetratingBtn = (
		try
		with redraw off(
			setCommandPanelTaskMode #modify
			SuspendEditing()
			select prepLeaf -- tends to break if not selected
			myVolSelect=Vol__Select()
			addModifier prepLeaf myVolSelect
			myVolSelect.type = 1
			myVolSelect.level = 2
			myVolSelect.volume = 3
			myVolSelect.node = prepBranch
			myEP=Edit_Poly ()
			addModifier prepLeaf myEP
			myEP.selectMode=1
			myEP.SetEPolySelLevel #face
			ResumeEditing() -- breaks if not in modify mode and editing is resumed before the conversion
			myEP.ConvertSelection #face #element requireAll:false
			newObjectName=prepLeaf.name+"_DetachedObject"
			newobj=myEP.DetachToObject newObjectName
			convertToPoly(prepLeaf)
			
		)
		catch(
			displayAutoErrorMessage()
			ResumeEditing()
		)
		if keyboard.escPressed do ResumeEditing()
	)
	/************************************************************************************************************************************************************
	create Spline Vert Array
	************************************************************************************************************************************************************/
	fn createSplineVertArray =(
		splineVertArray=#()
		if	(
				isvalidnode currSplineObj and (classof currSplineObj == SplineShape or classof currSplineObj == Line) and currSplineObj.modifiers.count == 0 and numKnots currSplineObj > 1
			) do (
					for s = 1 to (numSplines currSplineObj) do (
						for k = 1 to (numKnots currSplineObj s) do (
							knt = getKnotPoint currSplineObj s k
							individualVertArray=#(s,k,knt)
							append splineVertArray individualVertArray
						)--end k loop
					)--end s loop
			)
	)-- creates splineVertArray, #(s,k,knt)
	fn createMeshVertArray =(
		splineVertArray=#()
		if	(
			isvalidnode meshPivotObj and \
			(classof meshPivotObj == editable_poly ) \
			and meshPivotObj.modifiers.count == 0 and \
			polyop.getNumVerts meshPivotObj > 0
		) then (
			for v = 1 to (polyop.getNumVerts meshPivotObj) do (
				vertPos = polyop.getVert meshPivotObj v 
				individualVertArray=#(1,v,vertPos)
				append splineVertArray individualVertArray
			)
		) else (false)
	)
	global findClosestVert
	
	fn qSortCompareDistances a b = (
			if a.vertDist > b.vertDist then 1 else -1
	)
	fn updateProgBar stage:1 maxOperations:100000000.0 currentOperation:0.0 bar:1= (
		progBar = case bar of
		(
			1: PrepTools_rollout.pbProgress
			2: PaintIndividualPivots_rollout.pbIndividualPivots
			3: PaintLeavesAndTrees_rollout.pbTreeVertexPainter
			default: PrepTools_rollout.pbProgress
		)
		pbCol = clamp (((stage/pbNumberOfStages)-0.2)*255) 0 255 
		pbCol = [pbCol,pbCol,pbCol] as point3
		if ( currentOperation == maxOperations ) then (
				progBarAmount=0
			) else (
			progBarAmount=(currentOperation as float/maxOperations as float)*100.0
		)
		progBar.value = progBarAmount
		progBar.color = pbCol
		if keyboard.escPressed do PrepTools_rollout.pbProgress.value = 0
	)
	
	fn findClosestVert collectNewitems:false= (
		starttime= timestamp()
		suspendediting()
		try with undo off (
			if selectionSetIntPrep != 0 and selectionSetIntPrep <= selectionSets.count and splineVertArray.count > 0  then ( 
				global myBoxToLeafSelSetName=selectionSets[selectionSetIntPrep].name
				global finalMeshVertPairArray=#()
				global tempArray=#()
				global lowestSet=#()
				global FinalFullVertArray=#()
				if LeafArray.count > 100 and acceptSlowness==false then (
					acceptSlowness= (pivotProcede = querybox "Warning there are more than 100 items in your selection set. Breaking the selection set up in to smaller groups can increase processing speed and stability. This warning will only display once per session. \r\r Would you like to continue processing this group?" title:"Warning" beep:false)
				)
				else (
					pivotProcede=true
				)
				if pivotProcede then (
					global selectionsetName=selectionsets[selectionSetIntPrep].name -- set initially
					pbMaxOperations=LeafArray.count 	--getprogressbar totals 
					for m=1 to LeafArray.count do ( -- for each mesh
						tempArray=#()
						global currMesh=LeafArray[m]
						mVertCount= getNumVerts currMesh
						for s=1 to splineVertArray.count do ( -- for each mesh process each spline vertex
							for v=1 to mVertCount do ( -- for each mesh process each spline vertex compared with each model vertex
								currMeshVert = polyop.getVert currMesh v
								myDist=distance splineVertArray[s][3] currMeshVert 
								append tempArray  (objPiv obj:currMesh objVert:v spln:currSplineObj splnSubline:splineVertArray[s][1] splnVert:splineVertArray[s][2] splnVertPos:splineVertArray[s][3] vertDist:myDist )
							) --list is now populated with all vert combinations for 1 mesh
						) -- end spline loop
						qsort tempArray qSortCompareDistances -- sort those distances making the lowest entry the object with the shortest distance
						append FinalFullVertArray tempArray[1] -- append the lowest combination to the final full vert array
						updateProgBar stage:1.0 maxOperations:pbMaxOperations currentOperation:m
					)
					boxToLeaveSelSetArray=#()
					BBCheckState=true 
					pbMaxOperations=FinalFullVertArray.count
					for i=1 to  FinalFullVertArray.count do (
						currMeshObj=FinalFullVertArray[i].obj
						centerpivot currMeshObj
						findObjectAxis leaf:currMeshObj vertexID:FinalFullVertArray[i].objVert numberOfExpansions:expansionNumber processAllNormals:processAllMeshNormals
						AlignPivotTo currMeshObj newAxis
						updateProgBar stage:3.0 maxOperations:pbMaxOperations currentOperation:i 
					)
				) else (
					if splineVertArray.count == 0 do (
						-- hitlist issue
						messagebox "New pivots have not been created.\r\rYour pivot object has either been deleted, picked using the H scene object list hotkey, contains modifiers, is not an editable polygon object or has 0 vertices. \n"
						PrepTools_rollout.createNewLeafPivot.enabled=false
					) 
					updateDropDownList();
					PrepTools_rollout.selectionSetDropDown.items = dropList
					false
				)
			) -- end if selection set is valid, should be checked before function is called. this if is a fail safe to avoid an exception
		)-- end try 
		catch (
			displayAutoErrorMessage()
			resumeediting()
		)
		resumeediting()
	)-- end func
	
	fn paintLeaves = ( -- paints branches and leaves 
		try 
		with redraw off (
			global branchLocalBoundingBox=nodeLocalBoundingBox currBranchObj
			branchLocalBoundingBox = distance branchLocalBoundingBox[1] branchLocalBoundingBox[2]
			branchAngle=(normalize currBranchObj.transform[1])*[1.0,-1.0,1.0] -- Branch x axis vector (float 3) -- invert y for vert color 
			if branchAngle[3]<0 then (zSign=-1.0) else (zSign=1.0)
			branchAngle=((branchAngle+[1.0,1.0,1.0])/2.0)*255.0 -- Branch x axis vector (float 3) 
			branchAngle=[clamp branchAngle[1] 0 255,branchAngle[2],branchAngle[3]]
			global uv4Green=(branchAngle[2]*zSign)
			branchPos=convertWPtoUV pos:(currBranchObj.pos*[1,-1,1]) -- 0-1 range flip the y axis
			polyop.setVertColor currBranchObj -2  #all [branchAngle[1],0,0] -- set vert color for branch to branch position
			polyop.setVertColor currBranchObj 0  #all branchPos -- set vert color for branch to branch position
			polyop.setVertColor currBranchObj 3 #all [0,0,0] -- can be used for pixel shading if needed
			polyop.setVertColor currBranchObj 4 #all [0,uv4Green,0]
			fixUVNames currBranchObj
			fnpaintleavesMaxOp=leafArray.count  
		for i = 1 to leafArray.count do (
				currObj=leafArray[i]
				pivotPos=currObj.pos*255
				pivotPos=[ceil pivotPos[1],ceil pivotPos[2],ceil pivotPos[3]]
				xAxis=(((normalize currObj.transform[1])*.5)+.5)*255 -- leaf's x - axis
				xAxis=[clamp (xAxis[1]) 20 240, clamp (xAxis[2]) 20 240, clamp (xAxis[3]) 20 240] -- padding is added to avoid the lack of precision -- only works for near meshes
				if pivotpos[1]>0 then (pivotpos[1]+=xAxis[1]) else (pivotpos[1]-=xAxis[1])
				if pivotpos[2]>0 then (pivotpos[2]+=xAxis.y) else (pivotpos[2]-=xAxis.y)
				if pivotpos[3]>0 then (pivotpos[3]+=xAxis.z) else (pivotpos[3]-=xAxis.z)
				polyop.setVertColor currObj -2 #all [branchAngle[1],0,0]--branch - y axis
				polyop.setVertColor currObj 0 #all branchPos--branch position
				polyop.setVertColor currObj 3 #all pivotPos -- + x transform in the remainder
				polyop.setVertColor currObj 4 #all [pivotPos[3],uv4Green,0]--leaf pivot (ws), random value or constant bias scaled X with Z sign. use absolute value then scale bias to find x. Derive z from x and y to find z. Apply z sign to z channel.
				fixUVNames currObj
				updateProgBar maxOperations:fnpaintleavesMaxOp currentOperation:i bar:3
		)
		) catch(displayAutoErrorMessage())
	)
fn paintLocalValues paintVectorOrRotation:1	optimized:false=(
		try 
		with redraw off (
			localMaxOperations=LeafArray.count
			for i=1 to LeafArray.count do(
					currMesh = LeafArray[i]
					pivotPos=currMesh.pos*255
				if optimized==true then (
					polyop.setVertColor currMesh 3 #all [pivotPos[1],pivotPos[2],0]
					polyop.setVertColor currMesh 4 #all [pivotPos[3],(random 0.0 255.0),0]
				) else (
					localBoundingBox=nodeGetBoundingBox currMesh currMesh.transform
					localBoundingBoxDist= distance localBoundingBox[1] localBoundingBox[2]
		
					if paintVectorOrRotation == 1 then (
						myAngle=((((normalize currMesh.transform[1])*[1,-1,1])+1)/2)*255 -- Branch x axis vector --make the value 0-1 in unreal -- unreal inverts vert color y
					) else (
						myAngle= in coordsys world quatToEuler2 (inverse currMesh.rotation);
						myAngle= [myAngle.x,myAngle.y,myAngle.z]
						myAngle=(myAngle*(256.0/360.0))+128
					)
					polyop.setVertColor currMesh 0 #all myAngle
					polyop.setVertColor currMesh 3 #all pivotPos
					polyop.setVertColor currMesh 4 #all [pivotPos[3],(random 0.0 255.0),0]--leaf pivot (0-1), leaf z rotation
					--Paint alpha per vertex 
					for v=1 to (getNumVerts currmesh) do (
						if keyboard.escPressed do ResumeEditing()
						currVert=polyop.getVert currMesh v
						currVertBaseObj=polyop.getVert currMesh.baseobject v
						gradBBX=[0,localBoundingBox[1][1],0]
						gradBBXTwo=[0,localBoundingBox[2][1],0]
						gradBBY=[0,localBoundingBox[1][2],0]
						gradBBYTwo=[0,localBoundingBox[2][2],0]
						gradBBZ=[0,localBoundingBox[1][3],0]
						gradBBZTwo=[0,localBoundingBox[2][3],0]
						finXScale = (distance gradBBX gradBBXTwo)
						finYScale = (distance gradBBY gradBBYTwo)
						finZScale = (distance gradBBZ gradBBZTwo)
						finXVal= (pow ((distance [0,currVertBaseObj[1],0] gradBBX)/finXScale) leafWingFalloffPowerX ) + (pow((distance [0,currVertBaseObj[1],0] gradBBXTwo)/finXScale) leafWingFalloffPowerX)
						finYVal= (pow ((distance [0,currVertBaseObj[2],0] gradBBY)/finYScale) leafWingFalloffPowerY ) + (pow((distance [0,currVertBaseObj[2],0] gradBBYTwo)/finYScale) leafWingFalloffPowerY)
						finZVal= (pow ((distance [0,currVertBaseObj[3],0] gradBBZ)/finZScale) leafWingFalloffPowerZ ) + (pow((distance [0,currVertBaseObj[3],0] gradBBZTwo)/finZScale) leafWingFalloffPowerZ)
						distanceToPivot=distance currVert currMesh.pos/255
						finalAlpha= clamp (((pow (distanceToPivot*leafLengthFalloff)leafLengthFalloffPower) +(finYVal*leafWingFalloffY)+(finXVal*leafWingFalloffX)+(finZVal*leafWingFalloffZ))*255) 0 255
						polyop.setVertColor currmesh -2 v [finalAlpha,finalAlpha,finalAlpha] 
					)
				)
				fixUVNames currMesh
				updateProgBar maxOperations:localMaxOperations currentOperation:i bar:2
			)
		) catch(displayAutoErrorMessage())
	)

	fn checkSpline spline =  isvalidnode spline and (classof spline == SplineShape or classof spline == Line) and spline.modifiers.count == 0 -- and numKnots mySpline > 1
	
		--Creates LeafArray data 
		fn createLeavesFromSelectionSet selectionSetIntVar:selectionSetInt  message:true runupdateProgBar:false=(
			-- reinitialize value
			LeafArray=#()
			--check to see everything exists and is usuable 
			if (selectionSetIntVar != undefined and \ 
				selectionSetIntVar != 0 and \
				selectionSetIntVar <= selectionSets.count and \
				selectionSets[selectionSetIntVar].count > 0 and \
				not isDeleted selectionSets[selectionSetIntVar]
				) 
			then (
				-- collect all of the useable objects 
				operationCount=selectionSets[selectionSetIntVar].count
				for i=1 to operationCount where checkGeo selectionSets[selectionSetIntVar][i] == true do (
					if runupdateProgBar==true do (updateProgBar stage:1.0 maxOperations:operationCount currentOperation:i)
					append LeafArray selectionSets[selectionSetIntVar][i]
				)
				if LeafArray.count > 0 then (
					bLeafMessage=false; 
					bContinue = true;
				) 
				-- 0 objects were usable
				else (
					bLeafMessage=true
					if message==true then errorMessage()
					bContinue = false 
				)
			)
			-- if the selection set is invalid
			else (
				bLeafMessage=true
				if message==true then errorMessage()
				bContinue = false 
			)
		)	-- end create leaves from selection set
		
		-- First check to see if everything is valid and then paint the vertices
		fn paintLeavesAndBranches = (
			-- Creates leaf array and sets bContinue -- Stored in leafArray
			createLeavesFromSelectionSet ()
			-- check to see if branch is still valid 
			if bContinue then (
				bContinue = checkGeo currBranchObj --or not bPaintBranch
				if bContinue then  (
					paintLeaves () 
				)else (
					bBranchMessage = true; 
					errorMessage();
				) 
			)
		)

		fn concatNameAndNumber prefixString num width= (
			numString = num as string
			while (numString.count < width) do (
				numString = "0" + numString
			)
			return prefixString + numString
		)
	
		fn	seperateElementsIntoEditablePolys = (
						
			/*
				REWRITTEN - Chris Wood
			
				-loop through selected nodes
				-ignore non mesh type nodes
				-convert to mesh (to allow meshop methods needed to detach faces and process the detached object further)
				-remove the node's object offset
				-get the node's colour
				-while there are faces in the original mesh
				-get a list of faces in the same element as face 1
				-detach the faces in the element as a new mesh
				-create a new mesh-type node and set the mesh as the newly detached geometry
				-adjust the transform to preserve the location
				-convert the result to editable polys
				-ResetXForm, transform, scale, pivot and centre pivot
				-Collaspe mod stack
				-add new node to the element node list for this node
			
				-add original empty node to the delete list
				
				-loop through the element nodes for the node
				-set the colour based on the parent node
				
				-create a selection set from the element nodes array for this node
				
				-when all done, delete the now empty nodes
			*/
			
			SuspendEditing()
			try
			with redraw off (
				delNodesArray = #()
				for obj in $
				do (
					if (canConvertTo obj Editable_Mesh) then (
						ConvertToMesh obj
						elementNodesArray = #()
						
						--Reset object offset before splitting otherwise the offset is lost
						obj.transform = obj.objectTransform
						obj.objectOffsetPos = [0,0,0]
						obj.objectOffsetRot = Quat 0 0 0 1
						obj.objectOffsetScale = [1,1,1]
						
						--Get object wire colour for use as child objects' colours
						wirecolor=obj.wirecolor as point3
						wirecolorMax=amax #(wirecolor[1],wirecolor[2],wirecolor[3]) 
						wirecolor= wirecolor * 255.0/wirecolorMax
						newWireColor=wirecolor
						
						nodeName=obj.name
						
						while (meshop.getNumFaces obj > 0) do (
							elementFaces = meshop.getElementsUsingFace obj 1
							newMesh = meshop.detachFaces obj elementFaces delete:true asMesh:true
							prefixName = nodeName + "_Detached_"
							newNode = Editable_Mesh prefix:prefixName
							newNode.mesh = newMesh
							update newNode
							newNode.transform *= obj.transform
							convertTo newNode Editable_Poly
							ResetXForm newNode
							ResetTransform newNode
							ResetScale newNode
							ResetPivot newNode
							CenterPivot newNode
							
							maxOps.CollapseNode newNode true
							
							append elementNodesArray newNode
						)
						
						append delNodesArray obj
						
						wireColourStep=255.0/(elementNodesArray.count + 1)
						for elementNode in elementNodesArray
						do (
							newWireColor -= wireColourStep
							newWireColor=[clamp newWireColor[1] 0 255,clamp newWireColor[2] 0 255,clamp newWireColor[3] 0 255]
							elementNode.wirecolor=newWireColor as color							
						)
						
						selectionSetPrefix = nodeName + "_Detached_Collection_"
						selectionUniqueIndex = 1
						selectionSetName = concatNameAndNumber selectionSetPrefix selectionUniqueIndex 3
						
						isUniqueName = false
						while (isUniqueName == false)
						do (
							isUniqueName = true
							for existingSet in selectionSets
							do (
								if (existingSet.name == selectionSetName) then (
									selectionUniqueIndex += 1
									selectionSetName = concatNameAndNumber selectionSetPrefix selectionUniqueIndex 3
									isUniqueName = false
								)
							)							
						)
						
						selectionSets[selectionSetName] = elementNodesArray
						select selectionSets[selectionSetName]
					)
				)
				delete delNodesArray
				
				ResumeEditing()
			)
			catch (
				format "*** % ***\n" (getCurrentException())
				ResumeEditing()
			)
		) -- end seperateElementsIntoEditablePolys
		
		fn addSelectionPrefab branchVar selSetNameVar DataOptionsPrefVar= (
			append customSelectionGroupsArray  (customSelGroup  branch:branchVar selSetName:selSetNameVar DataOptionsPref:DataOptionsPrefVar)
		)
		fn deleteSelectionPrefab ArrayID = (
			deleteItem customSelectionGroupsArray ArrayID
		)
		fn paintSelectedItemsBlack prbar:2= (
			--selectionArray=selection as array
			global blackMaxOperations=selection.count
			if selection.count > 0 do
					for i=1 to (selection).count where checkGeo selection[i] == true do (
						currobj=selection[i]
						polyop.setVertColor currObj -2 #all [0,0,0]
						polyop.setVertColor currObj 0 #all [0,0,0]
						polyop.setVertColor currObj 3 #all [0,0,0]
						polyop.setVertColor currObj 4 #all [0,0,0]
						updateProgBar maxOperations:blackMaxOperations currentOperation:i bar:prbar
					)
			completeRedraw()	
		)

	-- Interface---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	rollout PrepTools_rollout "Prep Tools"
	(
		on PrepTools_rollout rolledUp state do (
			if state then 
			(
				bPrepTools=true
				adjustRolloutHeight()
			)
			else (
				bPrepTools=false 
				adjustRolloutHeight()
			)
		)
		button seperateElementsIntoEditablePolysBtn "Detach Selected Model's Elements" width:guiW tooltip:"This will detach every element in an object as another object."
		group "Detach Elements Based on Penetration" (
			pickbutton selectBranchDetachBtn "Pick Selection Obj"  across:2 width:(guiW/2-5)   align:#left filter:checkGeoDetachPen tooltip:"Pick an editable polygon object without any modifiers applied. Do not pick the same object for both the ''selection object'' and ''model to process'' options.\n\nNote: The selection mesh cannot have any holes."
			pickbutton selectLeafDetachBtn "Pick Model to Proc" align:#right  width:(guiW/2-5) filter:checkGeoDetachPen tooltip:"Pick an editable polygon object containing several elements without any modifiers applied. Do not pick the same object for both the ''selection object'' and ''model to process'' options.\n\nNote: The selection mesh cannot have any holes."
			button detachElementsInterpenetratingBtn "Detach Model Elements That Touch" width:guiW enabled:false tooltip:"This will seperate any elements that touch, or penetrate, the selection object's geometry into a seperate object.\n\nNote: This tool will not work properly if the selection object has any holes."
		)
		group "Generate New Pivot Points" (
			label mylabel10 "Pick Your Model Selection Set" align:#left
			
			dropdownlist  selectionSetDropDown "" items:dropList selection:0 height:10 tooltip:"Pick your model selection set. Press ''Manage'' to modify your selection sets and Update to get the latest sets." across:3 width:(guiW/2.25) 
			button updateSelectionSetList "Update" width:(guiW/4) align:#right offset:[10,0] tooltip:"Press this button to get the latest selection sets from Max."
			button manageSelectionSetList "Manage" width:(guiW/4) align:#right tooltip:"Press this button to manage your selection sets."
			label mylabel11 "Pick Leaf Pivot Obj" align:#left across:2
			radiobuttons rdoPivotReferenceType  labels:#( "Spline","Mesh")   tooltip:"Choose wheither you would like to constuct your pivots based off of the knots in a spline or the vertices in an editable poly object." columns:2
			pickbutton branchSplinePickBtn "Pick Spline"   across:2 align:#left filter:checkSpline width:(guiW/2-5) tooltip:"Pick a spline from the scene. The script will find the closest vertex pair between each model and each knot in the spline. Then it will move the objects pivot to the location of that vertex and orient the axis to match the model. The x-axis will point toward the center of the model from the desired vertex. The z-axis will point along the surface's normal. Processing time will increase as vertex counts rise.\r\rWarning:\rMake sure to not use the H list hotkey. The object must be picked from the viewport.\r\r
Tip:\rFor grass use a single a two knot spline placed well below your grass to increase calculation speeds."
			pickbutton branchPivMeshPickBtn "Pick Mesh"   align:#left filter:checkGeo width:(guiW/2-5) tooltip:"Pick a mesh. The script will find the closest vertex match between the models' and pivot model's vertices. Then it will orient the leaf's axis to the polygonal orientation and vertex position.\r\rWarning:\rMake sure to not use the H list hotkey. The object must be picked from the viewport.\r\r
Tip:\rFor grass use a editable polygon pivot point mesh with a single triangle placed well below your grass to increase calculation speeds." enabled:false
			label numAverLbl "Num of Face Norms to Avg Near Pivot"
			spinner spnNumberofNormalFaces range:[1,10000000,expansionNumber] type:#integer width:(guiW/2) align:#left across:2
			checkbox chkBxUseAllNormals "Average All" align:#center
			button createNewLeafPivot "Create New Pivots" width:guiW enabled:false tooltip:"This will generate a new pivot point for every editable polygon object in the selection set. The pivot will rest on the model vertex closest to a knot in the selected spline or vertex in the chosen editable poly object. The x-axes of the pivots will be oriented toward the averaged center of the meshes.
\rTip:\rFor grass use a two knot spline placed well below your grass to increase processing speeds."
			progressbar pbProgress "New Pivot Creation Progress" value:0 color:pbCol 
		)
		on chkBxUseAllNormals changed theState do (
			spnNumberofNormalFaces.enabled = not theState
			processAllMeshNormals = theState
		)
		
	fn checkListToActivateCreateNewPivots =(
		if droplist.count == 0 or selectionSetDropDown.selected==undefined or selectionSetIntPrep == 0 or selectionSetIntPrep > selectionSets.count then (
			false
		) else (
			true
		)
	)
			
		
	
	fn setSplineVertPivotMethod = (
		branchSplinePickBtn.enabled = true
		branchPivMeshPickBtn.enabled = false
		if (checkSpline currSplineObj) then (
			updateDropDownList();
			selectionSetDropDown.items = dropList;
			branchSplinePickBtn.text = truncateString ("Spline: " + currSplineObj.name as string) halfButtonStringlength
			if checkListToActivateCreateNewPivots() then (
				createNewLeafPivot.enabled =true; 
			) else (
				createNewLeafPivot.enabled =false
			) 
		)
		else (
			branchSplinePickBtn.text = "Pick Spline";
			currSplineObj=undefined
			createNewLeafPivot.enabled= false
			updateDropDownList();
			selectionSetDropDown.items = dropList;
		)
	)
	fn setMeshVertPivotMethod = (
		branchSplinePickBtn.enabled = false
		branchPivMeshPickBtn.enabled  = true
		updateDropDownList();
		selectionSetDropDown.items = dropList;
		if (checkGeo meshPivotObj) then (
			setbranchPivMeshPickBtntext();
			if checkListToActivateCreateNewPivots() do createNewLeafPivot.enabled = true 
		)
		else (
			branchPivMeshPickBtn.text = "Pick Mesh";
			meshPivotObj=undefined
			createNewLeafPivot.enabled  = false
		)
	)
	on rdoPivotReferenceType changed state do (
		if state == 1 then (
			setSplineVertPivotMethod ()
		) else (
			setMeshVertPivotMethod ()
		)
	)
	on branchSplinePickBtn picked spline do (
		updateDropDownList();
		selectionSetDropDown.items = dropList;
		branchSplinePickBtn.text = truncateString ("Spline: " + spline.name as string ) halfButtonStringlength
		currSplineObj=spline;
		
		bSplineMessage=false
		if checkListToActivateCreateNewPivots() then (
			createNewLeafPivot.enabled = true 
		)
	)
	on branchSplinePickBtn rightclick do (
		updateDropDownList();
		selectionSetDropDown.items = dropList;
		createNewLeafPivot.enabled = false 
		branchSplinePickBtn.text = "Pick Spline" 
		currSplineObj=undefined
		bSplineMessage=true
	)
	
	on branchPivMeshPickBtn picked pivMesh do (
		updateDropDownList();
		selectionSetDropDown.items = dropList;
		branchPivMeshPickBtn.text = truncateString ("Mesh: " + pivMesh.name as string ) halfButtonStringlength
		meshPivotObj=pivMesh;
		if checkListToActivateCreateNewPivots() then (
			createNewLeafPivot.enabled = true 
		)
	)
	on branchPivMeshPickBtn rightclick do (
		updateDropDownList();
		selectionSetDropDown.items = dropList;
		createNewLeafPivot.enabled = false 
		branchPivMeshPickBtn.text = "Pick Mesh" 
		meshPivotObj=undefined
	)
	on selectLeafDetachBtn rightclick  do (
		selectLeafDetachBtn.text = selectLeafDetachBtntext
		prepLeaf = undefined
		bprepLeafMessage  = true
		detachElementsInterpenetratingBtn.enabled = false
	)
		on selectLeafDetachBtn picked geo do
		( 
			 if isvalidobj geo do
			 ( 
				selectLeafDetachBtn.text = truncateString ("Model: " + geo.name as string ) halfButtonStringlength
				prepLeaf = geo
				bprepLeafMessage  = false 
				if isvalidobj prepBranch then detachElementsInterpenetratingBtn.enabled = true
			)
		)
		on selectBranchDetachBtn rightclick  do (
				selectBranchDetachBtn.text = selectBranchDetachBtntext 
				prepBranch= undefined 
				bprepBranchMessage  = true
				detachElementsInterpenetratingBtn.enabled = false
		)
		on selectBranchDetachBtn picked geo do
		( 
			 if isvalidobj geo do
			 ( 
				selectBranchDetachBtn.text = truncateString ("Sel obj: " + geo.name as string ) halfButtonStringlength
				prepBranch= geo
				bprepBranchMessage  = false 
				if isvalidobj prepLeaf then detachElementsInterpenetratingBtn.enabled = true
			)
		)
		on detachElementsInterpenetratingBtn pressed do (
			if bprepLeafMessage == false and bprepBranchMessage == false and isvalidobj prepBranch  and isvalidobj prepLeaf then (
				fnDetachElementsInterpenetratingBtn()
			)
			else (
				bprepBranchMessage=not isvalidobj prepBranch
				bprepLeafMessage= not isvalidobj prepLeaf 
				errorMessagePrep()
				 
				if bprepBranchMessage then (selectBranchDetachBtn.text = selectBranchDetachBtntext; prepBranch=undefined)
				if bprepLeafMessage then (selectLeafDetachBtn.text =selectLeafDetachBtntext; prepLeaf=undefined)
				detachElementsInterpenetratingBtn.enabled=false
				) 
		)

		fn setbranchPivMeshPickBtntext =(
			PrepTools_rollout.branchPivMeshPickBtn.text = truncateString ("Mesh: " + meshPivotObj.name as string) halfButtonStringlength
		)

	fn recreateBoundingBox = (
		suspendediting();
		try
		with redraw off (			
			selsetArrays=#()
			global b
			--copy old selection sets into an array
			if selectionSets.count > 0 do(			
					for i=1 to selectionSets.count do append selsetArrays #(selectionSets[i].name,#())
					for i=1 to selectionSets.count do (for j in selectionSets[i] do append selsetArrays[i][2] j)
			)

			count=0
			
			for Obj in leafarray do (
				count+=1
				pbMaxOperations=leafarray.count
				--for every leaf create a box if it hasn't been created yet
				b=convertToPoly(box())
				b.transform =Obj.transform 
				b.wirecolor=Obj.wirecolor
				oldname=Obj.name
				currLayer= Obj.INodeLayerProperties.layer
				currLayer.addnode b
				
					
				for j=1 to selsetArrays.count do (-- cycle groups
					for i in selsetArrays[j][2] do ( -- cycle objects in each array group
						if Obj == i do (
							append selsetArrays[j][2] b
						)
					) 
				)
				polyop.attach b Obj
				polyop.deleteFaces b #{1..6}
				b.name=oldname
				if meshPivotObj == obj then (
					meshPivotObj=b
					setbranchPivMeshPickBtntext()
				)
				--finalMeshVertPairArray[j] = b
				updateProgBar stage:1.0 maxOperations:pbMaxOperations currentOperation:count 
			)

			-- delete deleted items from array
			tempselsetArrays= deepcopy selsetArrays
			for i=1 to selsetArrays.count do (
				selsetArrays[i][2]=#()
			)
			pbMaxOperations=tempselsetArrays.count 	
			for i=1 to tempselsetArrays.count do (
				for j=1 to tempselsetArrays[i][2].count do (
					if isvalidobj tempselsetArrays[i][2][j] do append selsetArrays[i][2] tempselsetArrays[i][2][j] 
				)
				updateProgBar stage:2.0 maxOperations:pbMaxOperations currentOperation:i 
			)
			for i=1 to selsetArrays.count do (
				selectionsets[selsetArrays[i][1]]=selsetArrays[i][2] 
			)
		)catch(
			displayAutoErrorMessage();
			resumeediting();
		)
		resumeediting();
	)
		on createNewLeafPivot pressed do undo "Create New Pivots" on ( with redraw off (
			createLeavesFromSelectionSet selectionSetIntVar:selectionSetIntPrep;
			
			if rdoPivotReferenceType.state == 1 then (-- spline
				createSplineVertArray();
			) else (
				if createMeshVertArray() == false do ( 
					branchPivMeshPickBtn.text = "Pick Mesh"
					meshPivotObj=undefined
				)
			)			
			
			if LeafArray.count > 100 then (
				global newSelectionSetItems=#()
				global currSegLeafArrayPos=1
				global segmentedLeafArray=#(#())
				for i=1 to leafArray.count do (
					if (mod i 100.0)==0 do (
						currSegLeafArrayPos+=1
						append segmentedLeafArray (#())
					)
					append segmentedLeafArray[currSegLeafArrayPos] leafArray[i]
				)
				for i=1 to segmentedLeafArray.count do (
					leafArray=segmentedLeafArray[i]
					
					findClosestVert collectNewitems:true
				)
				recreateBoundingBox();
			)else (findClosestVert ();
				recreateBoundingBox();)
		))
			
		on seperateElementsIntoEditablePolysBtn pressed do (
			undo "Detach Elements" on (
				seperateElementsIntoEditablePolys ();
			)
		)
			
		on manageSelectionSetList pressed do (
			updateDropDownList();
			selectionSetDropDown.items = dropList
			
			macros.run "Edit" "namedSelSets"
			if checkListToActivateCreateNewPivots() == false do createNewLeafPivot.enabled =false  
		)
		-- update selection sets for menu gui 
		on selectionSetDropDown selected i do ( 
			updateDropDownList();
			selectionSetDropDown.items = dropList;			
			global selectionSetIntPrep=i; 
			bLeafPrepMessage=false;  
			if i !=0 and i <=selectionSets.count then (
				select selectionSets[i];
				if rdoPivotReferenceType.state==1 then (
				-- eval spline
					if isvalidnode currSplineObj then (
						createNewLeafPivot.enabled = true 
					)else (
						bLeafSelSetPrepMessage=true;
					)

				) else (
				if (checkGeo meshPivotObj) do (
						createNewLeafPivot.enabled = true 
					)
				)
			)  else ( -- selection set was not good
				createNewLeafPivot.enabled = false 
			)
		)

		on updateSelectionSetList pressed do (
			
			updateDropDownList();
			selectionSetDropDown.items = dropList;
			if checkListToActivateCreateNewPivots() == false do createNewLeafPivot.enabled=false
		)
	)
	
	---------------------------------------- INDIVIDUAL PIVOT PAINTER
	
	rollout PaintIndividualPivots_rollout "Per Object Painter"
	(
		on PaintIndividualPivots_rollout rolledUp state do (
			if state then 
			(
				bIndividual=true
				adjustRolloutHeight()
			)
			else (
				bIndividual=false 
				adjustRolloutHeight()
			)
		)
	
		group "Objects to Paint" (
			label mylabel10 "Pick Your Object Selection Set" align:#left
			dropdownlist  selectionSetDropDown "" items:dropList selection:0 height:10 tooltip:"Pick your selection set containing editable poly objects. Press Manage to modify your selection sets and Update to get the latest sets." across:3 width:(guiW/2.25) 
			button updateSelectionSetList "Update" width:(guiW/4) align:#right offset:[10,0] tooltip:"Press this button to get the latest selection sets from Max."
			button manageSelectionSetList "Manage" width:(guiW/4) align:#right tooltip:"Press this button to Manage your selection sets."
		)
		
		group "Alpha Channel Falloff Controls" (
			spinner spnleafLengthFalloff "3D dist to piv multiplier" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000, leafLengthFalloff]  --tooltip:"This multiplies a distance calculation from the pivot to the current vertex."
			spinner spnleafLengthFalloffPower "3D dist to piv contrast" fieldwidth: 75 scale:0.05 range:[0, 100000000, leafLengthFalloffPower]  
			spinner spnleafWingFalloffX "X multiplier" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000, leafWingFalloffX] 
			spinner spnleafWingFalloffPowerX "X contrast" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000,leafWingFalloffPowerX]  
			spinner spnleafWingFalloffY "Y multiplier" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000, leafWingFalloffY]  
			spinner spnleafWingFalloffPowerY "Y contrast" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000, leafWingFalloffPowerY]  
			spinner spnleafWingFalloffZ "Z multiplier" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000, leafWingFalloffZ]  
			spinner spnleafWingFalloffPowerZ "Z contrast" fieldwidth: 75 scale:0.05 range:[-100000000, 100000000, leafWingFalloffPowerZ]  
			label PreviewLbl "Preview: "   
			radiobuttons rdoShowVertColors labels:#( "Diffuse","Alpha", "Color") columns:3  tooltip:"Click on the active viewport to view new channel."
		)
	group "Paint Options" (
			checkbox chkbxUnrealFoliagePlacementTool "Optimize for Foliage Placement tool" tooltip:"Checking this box will pack all of the position information into uv channel 3 and 4. Random values will be generated and stored in uv channel 4 g. This option exists to provide more efficient models and to work around issues caused by a limited number of interpolaters.\n\nPlease note that vertex alpha and color information cannot be displayed on meshes placed with the Unreal Foliage Placement Tool. Using this option will insure that your models are optimized for that use case."
			button paintLeafAndBranches "Paint Current Selection Set" height:40 width:guiW enabled:false tooltip:"Paint the editable polygon objects in the selection set using the parameters above. The rotation value's can either be baked as a vector or the as the rotation value seen at the bottom of the 3ds max viewport."
			button paintSelectedBlackBtn  "Paint selected meshes black (no anim)" align:#left filter:checkGeo width:guiW tooltip:"Fill the vertex color, alpha and uv channels 3 and 4 with black. This lack of value can be referenced in the Unreal shader to isolate or remove animation from certain elements."
			progressBar pbIndividualPivots ""
		)
		group "Advanced Rotation Options" (
					radiobuttons rdoVectorOrRotation labels:#( "X-Axis Vector","0-1 Rot Per Axis") columns:2  tooltip:"Rotation will return a biased and scaled (0.0-1.0) version of the object's rotation as it is seen in the transform dialog box at the bottom of 3DS Max.\
X-Axis Vector will paint the normalized, biased and scaled (0.0-1.0) x-axis orientation vector. Finding the cross product of the properly transformed x-axis and wind vector will return a useable rotation axis."
			)

		fn checkListToActivateCreateNewPivotsIndividual =(
			if droplist.count == 0 or selectionSetDropDown.selected==undefined or selectionSetIntIndividual==undefined or selectionSetIntIndividual == 0 or selectionSetIntIndividual > selectionSets.count then (false) else (true)
		)
		on chkbxUnrealFoliagePlacementTool changed currstate do (
			rdoVectorOrRotation.enabled = not currstate
			spnleafLengthFalloff.enabled = not currstate
			spnleafLengthFalloffPower.enabled = not currstate
			spnleafWingFalloffX.enabled = not currstate
			spnleafWingFalloffPowerX.enabled = not currstate
			spnleafWingFalloffY.enabled = not currstate
			spnleafWingFalloffPowerY.enabled = not currstate
			spnleafWingFalloffZ.enabled = not currstate
			spnleafWingFalloffPowerZ.enabled = not currstate
			rdoShowVertColors.enabled = not currstate
			PreviewLbl.enabled = not currstate
			
			if currstate == true do (
				rdoShowVertColors.state = 1
				for i in leafarray do (
					i.showVertexColors = off
					i.vertexColorsShaded = on
				)
			) 
		)
	
		on paintSelectedBlackBtn pressed do  (paintSelectedItemsBlack prbar:2)
		on spnleafLengthFalloff changed val do(
			leafLengthFalloff= val
		)
		on spnleafLengthFalloffPower changed val do(
			leafLengthFalloffPower= val
		)
		on spnleafWingFalloffY changed val do(
			leafWingFalloffY= val
		)
		on spnleafWingFalloffPowerY changed val do(
			leafWingFalloffPowerY = val
		)
		on spnleafWingFalloffX changed val do(
			leafWingFalloffX= val
		)
		on spnleafWingFalloffPowerX changed val do(
			leafWingFalloffPowerX= val
		)
		on spnleafWingFalloffZ changed val do(
			leafWingFalloffZ= val
		)
		on spnleafWingFalloffPowerZ changed val do(
			leafWingFalloffPowerZ= val
		)
		on manageSelectionSetList pressed do (
			updateDropDownList();
			selectionSetDropDown.items = dropList
			macros.run "Edit" "namedSelSets"
			if checkListToActivateCreateNewPivotsIndividual()==false do paintLeafAndBranches.enabled = false
		)
		-- update selection sets for menu gui 
		on selectionSetDropDown selected i do ( 
			updateDropDownList();
			selectionSetDropDown.items = dropList;			
			global selectionSetIntIndividual=i; 
				createLeavesFromSelectionSet selectionSetIntvar:selectionSetIntIndividual

			bLeafMessage=false; 
			if checkListToActivateCreateNewPivotsIndividual() and leafArray.count > 0 then (
				select selectionSets[i];
				paintLeafAndBranches.enabled = true
			) else (paintLeafAndBranches.enabled = false)
		)
		on updateSelectionSetList pressed do (
			updateDropDownList();
			selectionSetDropDown.items = dropList;
			if checkListToActivateCreateNewPivotsIndividual()==false do paintLeafAndBranches.enabled = false
		)
		on paintLeafAndBranches pressed do (
			createLeavesFromSelectionSet selectionSetIntVar:selectionSetIntIndividual;
			if bContinue then (
				paintLocalValues paintVectorOrRotation:(rdoVectorOrRotation.state) optimized:(chkbxUnrealFoliagePlacementTool.state)
			) else (
				updateDropDownList();
				selectionSetDropDown.items = dropList;
				global selectionSetIntIndividual=0; 
				paintLeafAndBranches.enabled =false
			)
			completeRedraw()
		)

	
		
		on rdoShowVertColors changed state do (
			  case rdoShowVertColors.state  of (
				1:(
						for i=1 to leafArray.count do(
							if isvalidobj leafArray[i] do (
								leafArray[i].showVertexColors = off
								leafArray[i].vertexColorsShaded = on
							)
						)
							
				  ) 
				2:(
						for i=1 to leafArray.count do(
							if isvalidobj leafArray[i] do (
								resetObjectDisplay obj:leafArray[i]
								leafArray[i].vertexColorsShaded = off
								leafArray[i].showVertexColors = on
								leafArray[i].vertexColorType = #alpha
								
							)
						)
						completeRedraw()						
				  )
				3:(
						for i=1 to leafArray.count do(
							if isvalidobj leafArray[i] do (
								resetObjectDisplay obj:leafArray[i]
								leafArray[i].vertexColorsShaded = off
								leafArray[i].showVertexColors = on
								leafArray[i].vertexColorType =  0
								
							)
						) 
					completeRedraw()
				)
			)
			completeRedraw()
		)
	)

	-----------------------------PAINT LEAVES ROLLOUT
	rollout PaintLeavesAndTrees_rollout "Hierarchy Painter"
	(
		on PaintLeavesAndTrees_rollout rolledUp state do (
			if state then 
			(
				bVertexPainter=true
				adjustRolloutHeight()
			)
			else (
				bVertexPainter=false 
				adjustRolloutHeight()
			)
		)
		group "Current Parent/Child Selection" (
			label mylabel10 "Pick Your Child Selection Set" align:#left
			dropdownlist  selectionSetDropDown "" items:dropList selection:0 height:10 tooltip:"Pick your object selection set. Press Manage to modify your selection sets and Update to get the latest sets." across:3 width:(guiW/2.25) 
			button updateSelectionSetList "Update" width:(guiW/4) align:#right offset:[10,0] tooltip:"Press this button to get the latest selection sets from Max."
			button manageSelectionSetList "Manage" width:(guiW/4) align:#right tooltip:"Press this button to Manage your selection sets."
			pickbutton branchPickBtn "Pick Parent" align:#left filter:checkGeo width:guiW tooltip:"Select an editable polygon object without any modifiers.The parent object's pivot point information will be baked into the parent as well as the children. \r\rNote: \rObjects do not need to be linked together to be denoted as children or parents."
		)
		group "Batch Parent/Child Groups" (
			label CreateBranchLeafGLbl "Create Parent/Child Groupings" align:#left;
			dropdownlist  prefabListDropDown "" items:prefabListDropDownItemArray selection:0 height:10 tooltip:"Pick a prefab that you have created with the plus button."  width:75 across:3 width:(guiW/3)
			button addPrefabBtn "+"  width:(guiW/4) align:#right tooltip:"Press this button to create a new parent/child combination entry."
			button removePrefabBtn "-" width:(guiW/4) tooltip:"Press this button to remove the currently selected prefab entry."
		)
		group "Paint Options" (
			button paintLeafAndBranches "Paint Current Selections" height:40 width:guiW enabled:false tooltip:"Paint the current parent/child combination."
			button paintSelectedBlackBtn  "Paint selected meshes black (no anim)" align:#left filter:checkGeo width:guiW tooltip:"Paint the current selection black in all channels. This is typically used to mask motion for specific model elements like tree trunks."
			button batchProcessGroupsBtn  "Batch process parent/child groups" align:#left filter:checkGeo width:guiW tooltip:"Paint all of the objects listed in the batch parent/child groups."
			spinner spnShaderWSMultiplier "Max dist for parent piv" fieldwidth: 75 scale:0.05 range:[0, 100000000, shaderWSMultiplier] tooltip:"Set this distance to your farthest parent pivot point x, y or z location from world 0,0,0. Then enter this value into the shaders branchPosMultiplier_WS. Please maintain same value for all elements sharing the same shader settings. Smaller numbers will allow for greater accuracy. \r\rTip:\rMake sure that all tree branches share this same setting if they share the same material. If the model expands past the initial value set, use the batch process options to rescale the values." 
			progressbar pbTreeVertexPainter ""
		)
		label PreviewLbl "Preview: "  enabled:true
		radiobuttons rdoShowVertColors labels:#( "Diffuse","Alpha", "Color") columns:3  tooltip:"View the currently selected branch and leaf set's channel data."

		on rdoShowVertColors changed state do (
			  case rdoShowVertColors.state  of (
				1:(	
						if isvalidobj currBranchObj == true then (
							currBranchObj.showVertexColors = off
							currBranchObj.vertexColorsShaded = on
						)
						for i=1 to leafArray.count do(
							if isvalidobj leafArray[i] do (
								resetObjectDisplay obj:leafArray[i]
								leafArray[i].showVertexColors = off
								leafArray[i].vertexColorsShaded = on
							)
						) 
				  ) 
				2:(	
						if isvalidobj currBranchObj == true then (
								resetObjectDisplay obj:currBranchObj
								currBranchObj.showVertexColors = on
								currBranchObj.vertexColorType = #alpha
								currBranchObj.vertexColorsShaded = off
						)
						for i=1 to leafArray.count do(
							if isvalidobj leafArray[i] do (
								resetObjectDisplay obj:leafArray[i]
								leafArray[i].showVertexColors = on
								leafArray[i].vertexColorType = #alpha
								leafArray[i].vertexColorsShaded = off
							)
						)
						completeRedraw()						
				  )
				3:(
						if isvalidobj currBranchObj == true then (
							resetObjectDisplay obj:currBranchObj
							currBranchObj.vertexColorsShaded = off
							currBranchObj.showVertexColors = on
							currBranchObj.vertexColorType =  0
						)
						for i=1 to leafArray.count do(
							if isvalidobj leafArray[i] do (
								resetObjectDisplay obj:leafArray[i]
								leafArray[i].vertexColorsShaded = off
								leafArray[i].showVertexColors = on
								leafArray[i].vertexColorType =  0
							)
						)
					completeRedraw()						
				)
			)
		)
		fn checkListToActivateCreateNewPivotsH =(
			if droplist.count == 0 or selectionSetDropDown.selected==undefined or selectionSetInt == 0 or selectionSetInt > selectionSets.count then (false) else (true)
		)			
		on spnShaderWSMultiplier changed val do (
			global shaderWSMultiplier = val
		)
		
		fn updateprefabListDropDown bAdd:true =(
			if bAdd==true then ( 
				-- updates list of batch processed stuff and selects the latest branch groupings after creating it
				if isvalidnode currbranchobj and selectionSetDropDown.selected!=undefined then (
					append customSelectionGroupsArray (customSelGroup Branch:currbranchobj selSetName:(selectionSetDropDown.selected))
					prefabListDropDownItemArray=(for i=1 to customSelectionGroupsArray.count collect customSelectionGroupsArray[i].branch.name as string)  as array
					prefabListDropDown.items=prefabListDropDownItemArray 
					prefabListDropDown.selection=prefabListDropDown.items.count
					branchPickBtn.text="Parent mesh name: "+prefabListDropDownItemArray[prefabListDropDownItemArray.count]
				) else (
						message=""
						if isvalidnode currbranchobj == false do message = "Your current parent object is not a valid editable polygon object."
						if selectionSetDropDown.selected == undefined do message+="\n Please select a child selection set."
						messagebox message
				)
			)
			else ( -- if fn called as delete 
				if customSelectionGroupsArray.count!=0 and prefabListDropDown.selection != 0  do (
					--print (prefabListDropDownItemArray [prefabListDropDown.selection])
					deleteItem customSelectionGroupsArray (prefabListDropDown.selection)
					deleteItem (prefabListDropDownItemArray) (prefabListDropDown.selection)
					prefabListDropDown.selection=0
					selectionSetInt=undefined
					prefabListDropDown.items=prefabListDropDownItemArray 
					branchPickBtn.text="Pick Parent"
					currBranchObj=undefined 
					bBranchMessage=true
					paintLeafAndBranches.enabled=false
					selectionSetDropDown.selection=0
				)
			)
		)
		
		on batchProcessGroupsBtn pressed do (
			if customSelectionGroupsArray == undefined or customSelectionGroupsArray.count == 0 then (
				messagebox ("Please create at least one batch parent/child group with the options above.")
			) 
			else for i=1 to customSelectionGroupsArray.count do (
				-- validation of nodes occurs in paint paintLeavesAndBranches function
				currBranchObj = customSelectionGroupsArray[i].Branch
				-- find selection set int
				selListfound=false
				for j =1 to selectionsets.count while selListfound == false do (
					if selectionsets[j].name == customSelectionGroupsArray[i].selSetName then (selectionSetInt = j ; selListfound=true; ) 
				) 
				paintLeavesAndBranches message:false
			)
		)
		-- Called when a new batch group is selected from the drop down. Updates button and selection set prefab
		fn setAllSettings infoArrayIndex:undefined = (
			branchfound=false
			-- find match to selection set based on name. Name is unique and should not change unless the user does it. ID can change. 
			if isvalidobj customSelectionGroupsArray[infoArrayIndex].Branch and customSelectionGroupsArray[infoArrayIndex].Branch!= undefined then
			(	
				branchfound=true
				currBranchObj= customSelectionGroupsArray[infoArrayIndex].Branch
				select currbranchobj
				branchPickBtn.text=truncateString ("Parent mesh name: " + customSelectionGroupsArray[infoArrayIndex].Branch.name as string ) fullButtonStringlength
				selListfound=false
				for i =1 to selectionsets.count while selListfound == false do (
					if selectionsets[i].name == customSelectionGroupsArray[infoArrayIndex].selSetName then (selectionSetInt = i ; selListfound=true; ) 
				) 
				if selListfound == true then (
				selectionSetDropDown.selection=selectionSetInt
				selectMore selectionSets[selectionSetInt]
				createLeavesFromSelectionSet ()
				) 
			)	
			if branchfound==false or selListfound==false then (
				updateprefabListDropDown bAdd:false 
				messagebox ("Objects from your batch leaf group were missing. The grouping has been removed from the list.")
			)
		)
		
		on addPrefabBtn pressed do (
			updateprefabListDropDown	bAdd:true

		)
		on removePrefabBtn pressed do (
			updateprefabListDropDown	bAdd:false 
		)
		on prefabListDropDown selected i do (
			setAllSettings infoArrayIndex:i;
		)
		on manageSelectionSetList pressed do (
			updateDropDownList();
			selectionSetDropDown.items = dropList
			macros.run "Edit" "namedSelSets"
			if checkListToActivateCreateNewPivotsH() == false do paintLeafAndBranches.enabled = false
		)
		-- update selection sets for menu gui 
		on selectionSetDropDown selected i do ( 
			updateDropDownList();
			selectionSetDropDown.items = dropList;			
			selectionSetInt=i; 
			bLeafMessage=false; 
			if checkListToActivateCreateNewPivotsH() then (
				select selectionSets[i];
				createLeavesFromSelectionSet selectionSetIntVar:selectionSetInt;
				if checkgeo currBranchObj then paintLeafAndBranches.enabled = true
			) else (
				bLeafMessage=true;
				paintLeafAndBranches.enabled = false
			)
		)
		on updateSelectionSetList pressed do (
			updateDropDownList();
			selectionSetDropDown.items = dropList;
			if checkListToActivateCreateNewPivotsH() == false do paintLeafAndBranches.enabled = false
		)
		-- set values for branch when picked 
		on branchPickBtn picked geo do
		( 
			 if isvalidobj geo do
			 ( 
				prefabListDropDown.selection=0
				branchPickBtn.text = truncateString ("Parent mesh name: " + geo.name as string) fullButtonStringlength
				currBranchObj = geo
				bBranchMessage  = false 
				 createLeavesFromSelectionSet message:false
				 if bcontinue==true then (
						paintLeafAndBranches.enabled = true 
				)
			)
		)
		on branchPickBtn rightclick do ( 
			prefabListDropDown.selection=0
			branchPickBtn.text= "Pick Parent"; 
			currBranchObj = undefined
			paintLeafAndBranches.enabled =false
		)
		on paintSelectedBlackBtn pressed do  (paintSelectedItemsBlack prbar:3)
		on paintLeafAndBranches pressed do (
			paintLeavesAndBranches()
		)
	)
	
	---------------------------------------Help rollout
	rollout Help_rollout "Help/Notes" (
		on Help_rollout rolledUp state do (
			if state then 
			(
				bhelp=true
				adjustRolloutHeight()
			)
			else (
				bhelp=false 
				adjustRolloutHeight()
			)
		)
		label lbl5  "Pivot Painter version 1.0\nWritten by Jonathan Lindquist \nat Epic Games." align:#left  height:40 
		label lbl3 
"---------------------------------\r
Hierachy Painter Output\r
---------------------------------\r
Child data:\r
Vert Alpha: parent[X][X] (0-1) \r
Vert Color: parent position XYZ (0-1)\r
UV channel 3: child pivot position XY (WS)\r                      + child [X][X],[X][Y] (0-1)\r
UV channel 4 R: child pivot position Z (WS)\r                      + child [X][Z](0-1)\r
UV channel 4 G: parent[X][Y](0-1) * Z sign\r

Parent Information: \r
Vert Alpha:  parent[X][X] (0-1)\r
Vert Color: parent position XYZ (0-1)\r
UV channel 3: 0\r
UV channel 4 R: 0\r
UV channel 4 G: parent[X][Y] (0-1) * Z sign\r
---------------------------------\r
Per Object Painter Output\r
---------------------------------\r
With Optimize Off:\r
Vert alpha : Custom Falloff\r
Vertex color : X Vector (rotation) (0-1)\r
UV channel 3: Pivot position XY (WS)\r
UV channel 4 R: Pivot position Z (WS)\r
UV channel 4 G: Random value (0-1)\r

With Optimize On:\r
Vert alpha : Empty\r
Vertex color : Empty\r
UV channel 3: Pivot position XY(WS)\r
UV channel 4 R: Pivot position Z (WS)\r
UV channel 4 G: Random value (0-1)\n

" align:#left height:445
	)

	if foliage_floater != undefined then CloseRolloutFloater foliage_floater
	foliage_floater = newRolloutFloater "Pivot Painter" floaterWidth 800
	addRollout PrepTools_rollout foliage_floater
	addRollout PaintLeavesAndTrees_rollout foliage_floater
	addRollout PaintIndividualPivots_rollout foliage_floater
	addRollout Help_rollout foliage_floater 
	
	PaintIndividualPivots_rollout.open=bIndividual 
	PaintLeavesAndTrees_rollout.open=bVertexPainter 
	PrepTools_rollout.open=bPrepTools 
	Help_rollout.open=bHelp 
)
macros.run "Epic Tools" "EpicTools_PivotPainter"