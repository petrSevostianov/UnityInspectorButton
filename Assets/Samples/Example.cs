using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour {

    public InspectorButton _Print;

    public void Print() {
        Debug.Log("Hello");
    }

    public InspectorFileSelectButton _OpenPngFile;
    [InspectorFileDialogButtonInfo("png")]
    public void OpenPngFile(string path) {
        Debug.Log("Selected PNG file: " + path);
    }

    public InspectorFileSelectButton _SavePngFile;
    [InspectorFileDialogButtonInfo("png", "Save PNG file", InspectorFileDialogButtonInfoAttribute.FileDialogType.Save)]
    public void SavePngFile(string path) {
        Debug.Log("Selected PNG file: " + path);
    }

}
