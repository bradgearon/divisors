using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class Tile
{
    public Image Image;
    public Text Text;
    
    public Tile Top;
    public Tile Left;
    public Tile Right;
    public Tile Bottom;

    private byte _number = 0;
    public byte Number
    {
        get
        {
            return _number;
        }
        set
        {
            _number = value;
            Text.text = _number + string.Empty;
        }
    }
}
