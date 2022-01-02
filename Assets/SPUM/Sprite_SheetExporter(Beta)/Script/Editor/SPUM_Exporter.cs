using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[CustomEditor(typeof(SPUM_Exporter))]
[CanEditMultipleObjects]
public class SPUM_ExporterEditor : Editor
{
    //field list
    SerializedProperty _unitPrefab;
    SerializedProperty _unitType;
    SerializedProperty _separated;
    SerializedProperty _imageName;
    SerializedProperty _imageSize;
    SerializedProperty _fullSize;
    SerializedProperty _scaleFactor;
    SerializedProperty _frameRate;
    SerializedProperty _advanced;    
    SerializedProperty _camera;
	SerializedProperty _anim;
	SerializedProperty _objectPivot;
	SerializedProperty _objectNow;
	SerializedProperty _imgBG;
	SerializedProperty _bgSet;
	SerializedProperty frameNowNumber;
	SerializedProperty _animNum;
	SerializedProperty timer;
	SerializedProperty timerForSave;
	SerializedProperty useTimer;
	SerializedProperty _netAnimClip;
	SerializedProperty animNum;
    SerializedProperty animationClips;
	SerializedProperty _animNameList;
	SerializedProperty _animNameNow;
	SerializedProperty _textSaveList;



    //parameter list
    float tValue = 0;
    SPUM_Exporter SPB;
    AnimationClip tAnimSave;
    float tAnimTimer;
    float tAnimTimerFactor;
    float timeSave;
    float tValuee = 0 ;
    bool objectSelectionFoldout = false;
    int objectSelectionToolbar = 0;

    void OnEnable()
    {

        // Fetch the objects from the GameObject script to display in the inspector
        _unitPrefab = serializedObject.FindProperty("_unitPrefab");
        _unitType = serializedObject.FindProperty("_unitType");
        _separated = serializedObject.FindProperty("_separated");
        _imageName = serializedObject.FindProperty("_imageName");
        _imageSize = serializedObject.FindProperty("_imageSize");
        _fullSize = serializedObject.FindProperty("_fullSize");
        _scaleFactor = serializedObject.FindProperty("_scaleFactor");
        _frameRate = serializedObject.FindProperty("_frameRate");
        _advanced = serializedObject.FindProperty("_advanced");
        _camera = serializedObject.FindProperty("_camera");
        _anim = serializedObject.FindProperty("_anim");
        _objectPivot = serializedObject.FindProperty("_objectPivot");
        _objectNow = serializedObject.FindProperty("_objectNow");
        _imgBG = serializedObject.FindProperty("_imgBG");
        _bgSet = serializedObject.FindProperty("_bgSet");
        _animNum = serializedObject.FindProperty("_animNum");
        timer = serializedObject.FindProperty("timer");
        timerForSave = serializedObject.FindProperty("timerForSave");
        useTimer = serializedObject.FindProperty("useTimer");
        _netAnimClip = serializedObject.FindProperty("_netAnimClip");
        animNum = serializedObject.FindProperty("animNum");
        animationClips = serializedObject.FindProperty("animationClips");
        _animNameList = serializedObject.FindProperty("_animNameList");
        _animNameNow = serializedObject.FindProperty("_animNameNow");
        _textSaveList = serializedObject.FindProperty("_textSaveList");

    }
    

    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        SPB = (SPUM_Exporter)target;
        // base.OnInspectorGUI();
        // EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(_unitPrefab);
        EditorGUILayout.PropertyField(_unitType);
        EditorGUILayout.PropertyField(_separated);
        EditorGUILayout.PropertyField(_imageName);
        EditorGUILayout.PropertyField(_imageSize);
        EditorGUILayout.PropertyField(_fullSize);
        EditorGUILayout.PropertyField(_scaleFactor);
        EditorGUILayout.PropertyField(_frameRate);
        if(!SPB._advanced)
        {
            EditorGUILayout.PropertyField(_advanced, new GUIContent("Advanced Settings Show"));
        }
        else
        {
            EditorGUILayout.PropertyField(_advanced, new GUIContent("Advanced Settings Off"));
            EditorGUILayout.HelpBox("Editing is not recommended.",MessageType.Warning);
            EditorGUILayout.PropertyField(_camera);
            EditorGUILayout.PropertyField(_anim);
            EditorGUILayout.PropertyField(_objectPivot);
            EditorGUILayout.PropertyField(_objectNow);
            EditorGUILayout.PropertyField(_imgBG);
            EditorGUILayout.PropertyField(_bgSet);
            EditorGUILayout.PropertyField(_animNum);
            EditorGUILayout.PropertyField(timer);
            EditorGUILayout.PropertyField(timerForSave);
            EditorGUILayout.PropertyField(useTimer);
            EditorGUILayout.PropertyField(_netAnimClip);
            EditorGUILayout.PropertyField(animNum);
            EditorGUILayout.PropertyField(animationClips);
            EditorGUILayout.PropertyField(_animNameList);
            EditorGUILayout.PropertyField(_animNameNow);
            EditorGUILayout.PropertyField(_textSaveList);
        }
        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();

        

        if (GUILayout.Button("Make Sprite Images",GUILayout.Height(50))) 
        {
            if(!SPB.useTimer)
            {
                Debug.Log("Starting Export Sprite Sheets...");
                SPB.StartExport();
                SPB.animNum = 0;
                SPB.animationClips = SPB._anim.runtimeAnimatorController.animationClips;
                SPB._textSaveList.Clear();
                SPB._netAnimClip = true;
            }
        }

        if(SPB._unitPrefab!=null)
        {
            if (GUILayout.Button("Remove Object",GUILayout.Height(50))) 
            {
                Debug.Log("Removed Prefab Object!!");
                SPB._unitPrefab = null;
                SPB._imageName = "";
            }
        }

        
  
        // objectSelectionFoldout = GUILayout.Toggle(objectSelectionFoldout, "" + (objectSelectionFoldout ? "▼ Object selection ▼" : "► Object selection ◄"), "Button", GUILayout.MaxWidth(Screen.width), GUILayout.Height(25));
  
        // if (objectSelectionFoldout)
        // {

        //     GUILayout.Space(5);     //Space before a text box

        //     GUILayout.Box("Select lenght:");

        //     GUILayout.Space(5);     //Space after a text box and before a toolbox

        //     string[] stationSelectionToolbarOptions = new string[] { "Object A", "Object B", "ObjectC" };

        //     objectSelectionToolbar = GUILayout.Toolbar(objectSelectionToolbar, stationSelectionToolbarOptions, GUILayout.MinWidth(Screen.width), GUILayout.Height(50));

        //     GUILayout.Space(5);

        //     switch (objectSelectionToolbar)
        //     {
        //         case 0:

        //             GUILayout.BeginHorizontal();

        //             if (GUILayout.Button("Place object", GUILayout.Height(25)))
        //             {
                
                
        //             }

        //             if (GUILayout.Button("Undo", GUILayout.Height(25)))
        //             {
                

        //             }
        //             GUILayout.EndHorizontal();

        //             GUILayout.Space(20);

        //             GUILayout.Box("Information:", GUILayout.MaxWidth(Screen.width), GUILayout.Height(50));

        //             GUILayout.Space(20);

        //             break;
        //     }

        // }

        // EditorGUILayout.EndVertical();

        // Update
        // image size sync
        SPB._imgBG.sizeDelta = new Vector2( SPB._imageSize.x, SPB._imageSize.y );
        if(SPB._unitPrefab==null)
        {
            SPB.CheckObjNow();
        }
        else
        {
            SPB.MakeObjNow();
            SPB._objectPivot.transform.localScale = new Vector3(SPB._scaleFactor,SPB._scaleFactor,SPB._scaleFactor);
        }

        if(SPB.useTimer)
        {
            float tTimer = Time.realtimeSinceStartup - timeSave;
            timeSave = Time.realtimeSinceStartup;
            SPB.timer += tTimer;

            if(SPB.timer > SPB.timerForSave)
            {
                
                tValue += tAnimTimerFactor;
                // Debug.Log(tValue);
                SPB.timer = 0;

                SPB.frameNowNumber++;
                if(SPB.frameNowNumber >= SPB._frameNumber)
                {
                    SPB.frameNowNumber = 0;
                    tValue = 0;
                    SPB.animNum++;
                    SPB.useTimer = false;

                    if(SPB.animNum < SPB._animNameNow.Count)
                    {
                        if(SPB._separated) 
                        {
                            SPB._sepaName = tAnimSave.name;
                            SPB.MakeScreenShotFile();
                        }
                        SPB._netAnimClip = true;
                    }
                    else
                    {
                        // Debug.Log(animNum);
                        if(!SPB._separated)
                        SPB.MakeScreenShotFile();
                        
                        SPB.PrintEndMessage();
                    }
                }
                
                if(SPB.animNum >= SPB._animNameNow.Count)
                {
                    ExporterReset();
                }
                else 
                {
                    ExporterShot();
                    SPB.SetScreenShot();
                }
                
            }
        }

        if(SPB._netAnimClip)
        {
            SPB._netAnimClip = false;
            AnimationClip tAnim = null;
            foreach( var obj in SPB.animationClips)
            {
                if(obj.name == SPB._animNameNow[SPB.animNum])
                {
                    tAnim = obj;
                }
            }
            
            if(tAnim == null) return;

            tAnimSave = tAnim;
            tAnimTimer = tAnimSave.length;
            tAnimTimerFactor = 1f / (SPB._frameRate*1f);
            // Debug.Log(tAnimTimerFactor);
            SPB._frameNumber = (int)(tAnimTimer / tAnimTimerFactor);
            // Debug.Log(SPB._frameNumber);
            Debug.Log("[[Generating : "+ tAnimSave.name + " || Time Length : " + tAnimSave.length + " sec " + "|| Frame Numbers : "+ SPB._frameNumber+"]]");
            SPB.frameNowNumber = 0;
            tValue = 0;
            SPB.timer = 0;
            SPB.useTimer = true;
            timeSave = Time.realtimeSinceStartup;

            SPB._textSaveList.Clear();
            ExporterShot();
            SPB.SetScreenShot();
        }
    }

    public void ExporterShot()
    {
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(SPB._anim.gameObject,tAnimSave,tValue);
        AnimationMode.EndSampling();
    }

    public void ExporterReset()
    {
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(SPB._objectNow,tAnimSave,0);
        AnimationMode.EndSampling();
        AnimationMode.StopAnimationMode();
    }

}
