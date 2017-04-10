using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public GridPositionField Source;
    public GridPositionField Destination;

    public IntegerField Cluster;
    public IntegerField Layers;

    public Dropdown DdlMaps;
    public Dropdown DdlLayers;

    public Text ClusterTime;
    public Text HPAStarTime;
    public Text AStarTime;

    //Bool that says which last gridpoint we set between source and destination
    private bool sourceSet = false;

    private void Start()
    {
        //Highlight tile being selected
        HighlightPositionSelector();
    }

    //Sets the next position between source and destination
    public void SetPosition(GridTile pos)
    {
        GridPositionField field = sourceSet ? Destination : Source;
        field.SetPositionField(pos);

        sourceSet = !sourceSet;
        HighlightPositionSelector();
    }

    //Highlight the input fields based on the next one to set
    public void HighlightPositionSelector()
    {
        Source.GetParent().GetComponent<Image>().enabled = !sourceSet;
        Destination.GetParent().GetComponent<Image>().enabled = sourceSet;
    }

    //Fill the dropdown of maps
    public void FillMaps(List<string> MapNames)
    {
        foreach (string s in MapNames)
            DdlMaps.options.Add(new Dropdown.OptionData(s));

        //Selects first
        DdlMaps.value = 0;
        DdlMaps.RefreshShownValue();
    }

    public void FillLayers(int MaxLayer)
    {
        DdlLayers.ClearOptions();
        for(int i = 0; i <= MaxLayer; ++i)
        {
            DdlLayers.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        DdlLayers.value = MaxLayer;
        DdlLayers.RefreshShownValue();
    }

}

[Serializable()]
public class GridPositionField
{
    public InputField X;
    public InputField Y;

    public GridTile GetPositionField()
    {
        return new GridTile(
            int.Parse(X.text), int.Parse(Y.text)
        );
    }

    public void SetPositionField(GridTile pos)
    {
        X.text = pos.x.ToString();
        Y.text = pos.y.ToString();
    }

    public Transform GetParent()
    {
        return X.transform.parent;
    }
}

[Serializable()]
public class IntegerField
{
    public InputField Field;

    public int GetValue()
    {
        return int.Parse(Field.text);
    }
}