using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;

public class LoadBrowser : MonoBehaviour
{
    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    public Editor editor;

    public void Start()
    {
        //// Set filters (optional)
        //// It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        //// if all the dialogs will be using the same filters
        //FileBrowser.SetFilters(true, new FileBrowser.Filter(".xml", ".xml"));

        //// Set default filter that is selected when the dialog is shown (optional)
        //// Returns true if the default filter is set successfully
        //// In this case, set Images filter as the default filter
        //FileBrowser.SetDefaultFilter(".xml");

        //// Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        //// Note that when you use this function, .lnk and .tmp extensions will no longer be
        //// excluded unless you explicitly add them as parameters to the function
        //FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".mp3", ".ogg", ".flac", ".aiff", ".flp", ".asd", ".mid", ".flp~",".wav");

        //// Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        //// It is sufficient to add a quick link just once
        //// Name: Users
        //// Path: C:\Users
        //// Icon: default (folder icon)
        //FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        //// Coroutine example
        ////StartCoroutine(ShowLoadDialogCoroutine());
    }

    public void ShowBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter(".xml", ".xml"));
        FileBrowser.SetDefaultFilter(".xml");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".mp3", ".ogg", ".flac", ".aiff", ".flp", ".asd", ".mid", ".flp~", ".wav");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Import .dspreset or .xml", "Import");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            //// Read the bytes of the first file via FileBrowserHelpers
            //// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            //// Or, copy the first file to persistentDataPath
            //string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            //FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
            //editor.AddSamplePath(FileBrowser.Result[0]);
            editor.LoadFromPath(FileBrowser.Result[0]);
        }
    }
}