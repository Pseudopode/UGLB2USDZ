using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using Shibuya24.Utility;
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using B83.Win32;
#endif

public class GLB2USDZ : MonoBehaviour
{
    // Start is called before the first frame update

    public Text fileText;
    public Button buttonConvert;

    public string filepath = "";

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    DropInfo dropInfo = null;
#endif

    void Start()
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // Required once
        UniDragAndDrop.Initialize();

        UniDragAndDrop.onDragAndDropFilePath = x =>
        {
            Debug.Log("File Dropped: " + x);
            fileText.text = "File: " + x;
            filepath = x;
            if (x.ToLower().EndsWith(".glb"))
            {
                enableConvertButton();
            }
            else
            {
                disableConvertButton();
            }
        };
#endif
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    class DropInfo
    {
        public string file;
        public Vector2 pos;
    }
    void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;

    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        string file = "";
        // scan through dropped files and filter out supported image types
        foreach (var f in aFiles)
        {
            var fi = new System.IO.FileInfo(f);
            var ext = fi.Extension.ToLower();
            filepath = f;
            if (ext == ".glb")
            {
                file = f;

                UnityEngine.Debug.Log("Dropped files");
                fileText.text = "File: " + file;

                break;
            }
        }
        // If the user dropped a supported file, create a DropInfo
        if (file != "")
        {
            var info = new DropInfo
            {
                file = file,
                pos = new Vector2(aPos.x, aPos.y)
            };
            dropInfo = info;
        }
    }

    private void OnGUI()
    {
        DropInfo tmp = null;
        if (Event.current.type == EventType.Repaint && dropInfo != null)
        {
            tmp = dropInfo;
            dropInfo = null;
        }
    }
#endif

        // Update is called once per frame
        void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    public void enableConvertButton()
    {
        buttonConvert.GetComponent<Button>().interactable = true;
    }

    public void disableConvertButton()
    {
        buttonConvert.GetComponent<Button>().interactable = false;
    }

    public void startGLB2USDZConversion()
    {
        fileText.text = "Wait for file conversion...";
        startProcess();
    }

    public void startProcess()
    {
        try
        {
            Process myProcess = new Process();
            myProcess.EnableRaisingEvents = true;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
           /* myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            myProcess.StartInfo.CreateNoWindow = false;
            myProcess.StartInfo.UseShellExecute = false;*/
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.EnableRaisingEvents = true;
            myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
            string path = Application.dataPath + "/../pxr_usd_min_alembic1710_py27_win64/run_usdzconvert.cmd " + filepath;
            myProcess.StartInfo.Arguments = "/c " + path;
            UnityEngine.Debug.Log("CommandLine: " + myProcess.StartInfo.Arguments);

            
            myProcess.Start();
            myProcess.WaitForExit();
            string output = myProcess.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log((output));

            int ExitCode = myProcess.ExitCode;
            print("Exit code : " + ExitCode);
            if(ExitCode == 0)
                fileText.text = "<color=\"green\">File conversion finished!</color>";
            else
                fileText.text = "<color=\"red\">An other as occured...</color>";
        }
        catch (Exception e)
        {
            print(e);
        }
    }
}
