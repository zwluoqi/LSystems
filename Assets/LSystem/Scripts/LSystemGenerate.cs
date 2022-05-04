using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LSystem.Scripts;
using UnityEngine;

public class GenerateMeshData
{
    public List<Vector3> vector3s = new List<Vector3>();
    public List<int> triangents = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    
    public List<SubMeshData> subMeshDatas = new List<SubMeshData>();
}

public class SubMeshData:ICloneable
{
    public Vector3 centerPos;
    public List<Vector3> vector3s = new List<Vector3>();
    public object Clone()
    {
        SubMeshData newSub = new SubMeshData();
        newSub.vector3s.AddRange(vector3s);
        newSub.centerPos = this.centerPos;
        return newSub;
    }

    public void Normalize()
    {
        this.centerPos = vector3s[0];
        for (int i = vector3s.Count-1; i >= 0; i--)
        {
            vector3s[i] -= vector3s[0];
        }
    }
}

// https://www.bilibili.com/read/cv3006796
public class LSystemGenerate
{
    private IGenerateImp iGenerateImp; 
    public GenerateMeshData Generate(ShapeSetting shapeSetting)
    {
        iGenerateImp = CreateGenerate(shapeSetting.generateType);
        iGenerateImp.Generate(shapeSetting);
        iGenerateImp.SaveFile(shapeSetting);
        return iGenerateImp.generateMeshData;
    }

    private IGenerateImp CreateGenerate(GenerateType shapeSettingGenerateType)
    {
        switch (shapeSettingGenerateType)
        {
            case GenerateType.Demo:
                return new DemoGenerateImp();
            case GenerateType.FactalPlant:
                return new FactalPlantGenerateImp();
            case GenerateType.Template:
                return new TemplateGenerateImp();
            break;
            default:
                throw new NotImplementedException();
        }
    }
}


public class LsystemEnv
{
    public Vector3 up = Vector3.up;
    public  Vector3 right = Vector3.right;
    public  Vector3 pos = Vector3.zero;
    public float lengthScale = 1.0f;
    public float widthIncrement = 0;
    public bool startSavePos = false;
    public SubMeshData subMeshData;

    public int id = 0;
    private static int idGenerater = 0;
    public LsystemEnv(LsystemEnv curEvn)
    {
        this.id = idGenerater++;
        up = curEvn.up;
        right = curEvn.right;
        pos = curEvn.pos;
        lengthScale = curEvn.lengthScale;
        widthIncrement = curEvn.widthIncrement;
        startSavePos = curEvn.startSavePos;
        if (curEvn.subMeshData != null)
        {
            subMeshData = (SubMeshData)curEvn.subMeshData.Clone();
        }
    }

    public LsystemEnv()
    {
        this.id = idGenerater++;
    }

    public override string ToString()
    {
        return "up:" + up + ",righe:" + right + ",pos" + pos;
    }
}

public abstract class IGenerateImp
{
    private Vector3[] rect = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
    };
    private Vector3[] forwardRect = new Vector3[]
    {
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
    };

    private Vector3[] tmpRect = new Vector3[8];


    private LsystemEnv curEvn = new LsystemEnv();
    private Stack<LsystemEnv> _stack = new Stack<LsystemEnv>();
    private StringBuilder _stringBuilder = new StringBuilder();
    public abstract void  Generate(ShapeSetting shapeSetting);
    public GenerateMeshData generateMeshData = new GenerateMeshData();

    protected  void UpdatePos(ShapeSetting shapeSetting)
    {
        if (curEvn.startSavePos)
        {
            curEvn.subMeshData.vector3s.Add(curEvn.pos);
        }
        var length = shapeSetting.size.y * curEvn.lengthScale;
        curEvn.pos += curEvn.up * length;
        if (curEvn.startSavePos)
        {
            _stringBuilder.AppendLine("AddPos:" + curEvn.ToString());
        }
        else
        {
            _stringBuilder.AppendLine("UpdatePos:" + curEvn.ToString());
        }
    }

    void UpdateRect(ShapeSetting shapeSetting)
    {
        var forward = Vector3.Cross(curEvn.right, curEvn.up);

        var length = shapeSetting.size.y * curEvn.lengthScale;
        
        // var scale = Mathf.Lerp(1, 0.25f, _stack.Count*1.0f / shapeSetting.maxIter);
        var scale = 1;
        var width = (shapeSetting.size.x + curEvn.widthIncrement)*scale;
        rect[0] = curEvn.pos + curEvn.right*width*(-0.5f)+curEvn.up*0-forward * width*0.5f;
        rect[1] = curEvn.pos + curEvn.right*width*0.5f+curEvn.up*0-forward * width*0.5f;
        rect[2] = curEvn.pos + curEvn.right*width*0.5f+curEvn.up*length-forward * width*0.5f;
        rect[3] = curEvn.pos + curEvn.right*width*(-0.5f)+curEvn.up*length-forward * width*0.5f;
        
        
        for (int i = 0; i < 4; i++)
        {
            forwardRect[i] = rect[i] + forward * width;
        }
    }

    protected  void AddCell(ShapeSetting shapeSetting)
    {
        UpdateRect(shapeSetting);

        int startIndex = generateMeshData.vector3s.Count;

        for (int i = 0; i < 4; i++)
        {
            tmpRect[i] = rect[i];
        }
        for (int i = 0; i < 4; i++)
        {
            tmpRect[i+4] = forwardRect[i];
        }
        
        //0,1,2,3
        AddCellFace(startIndex, 0, 1, 2, 3);
        //1,5,6,2
        AddCellFace(startIndex, 1,5,6,2);
        //5,4,7,6
        AddCellFace(startIndex, 5,4,7,6);
        //4,0,3,7
        AddCellFace(startIndex, 4,0,3,7);
        //3,2,6,7
        AddCellFace(startIndex, 3,2,6,7);
        //5,1,0,4
        AddCellFace(startIndex, 5,1,0,4);
        
        _stringBuilder.AppendLine("AddCell:"+curEvn.ToString());
    }

    private void AddCellFace(int startIndex,int i0, int i1, int i2, int i3)
    {
        startIndex = generateMeshData.vector3s.Count; 
        generateMeshData.vector3s.Add(tmpRect[i0]);
        generateMeshData.vector3s.Add(tmpRect[i1]);
        generateMeshData.vector3s.Add(tmpRect[i2]);
        generateMeshData.vector3s.Add(tmpRect[i3]);
        
        generateMeshData.uvs.Add(new Vector2(0,0));
        generateMeshData.uvs.Add(new Vector2(1,0));
        generateMeshData.uvs.Add(new Vector2(0,1));
        generateMeshData.uvs.Add(new Vector2(1,1));
        
        
        generateMeshData.triangents.Add(startIndex+0);
        generateMeshData.triangents.Add(startIndex+3);
        generateMeshData.triangents.Add(startIndex+2);
        
        generateMeshData.triangents.Add(startIndex+0);
        generateMeshData.triangents.Add(startIndex+2);
        generateMeshData.triangents.Add(startIndex+1);
    }

    
      
    
    // + Turn left by angle δ, using rotation matrix RU(δ).
    // − Turn right by angle δ, using rotation matrix RU(−δ).
    // & Pitch down by angle δ, using rotation matrix RL(δ).
    // ∧ Pitch up by angle δ, using rotation matrix RL(−δ).
    // \ Roll left by angle δ, using rotation matrix RH(δ).
    // / Roll right by angle δ, using rotation matrix RH(−δ).
    // | Turn around, using rotation matrix RU (180◦ ).
    
    protected void RotationF(ShapeSetting shapeSetting)
    {
        Rotation(0,1, shapeSetting.angle);
    }

    
    protected  void RotationB(ShapeSetting shapeSetting)
    {
        Rotation(0,1, -shapeSetting.angle);
    }
    
        
    void Rotation(float strength,float frequency,float angle)
    {
        var noise = strength*Unity.Mathematics.noise.snoise(frequency*curEvn.pos);
        noise = (noise + 1) * 0.5f;
        var dirAngle = Mathf.Lerp(-Mathf.PI, Mathf.PI, noise);
        Vector3 dir = new Vector3(Mathf.Sin(dirAngle), 0, Mathf.Cos(dirAngle));
        var rotation = Quaternion.AngleAxis(angle, dir);
        curEvn.up =  (rotation * curEvn.up).normalized;
        curEvn.right = (rotation * curEvn.right).normalized;
        _stringBuilder.AppendLine("Rotation:"+curEvn.ToString());
    }

    protected void TurnBack()
    {
        var forward = Vector3.Cross(curEvn.right, curEvn.up);
        var rotation = Quaternion.AngleAxis(180,forward);
        curEvn.up =  (rotation * curEvn.up).normalized;
        curEvn.right = (rotation * curEvn.right).normalized;
        _stringBuilder.AppendLine("TurnBack:"+curEvn.ToString());
    }
    
    protected void Turn(float angle)
    {
        var forward = Vector3.Cross(curEvn.right, curEvn.up);
        var rotation = Quaternion.AngleAxis(angle, forward);
        curEvn.up =  (rotation * curEvn.up).normalized;
        curEvn.right = (rotation * curEvn.right).normalized;
        _stringBuilder.AppendLine($"Turn({angle}):"+curEvn.ToString());
    }
    protected void Pitch(float angle)
    {
        var rotation = Quaternion.AngleAxis(angle, curEvn.right);
        curEvn.up =  (rotation * curEvn.up).normalized;
        curEvn.right = (rotation * curEvn.right).normalized;
        _stringBuilder.AppendLine($"Pitch({angle}):"+curEvn.ToString());
    }
    protected void Roll(float angle)
    {
        var rotation = Quaternion.AngleAxis(angle, curEvn.up);
        curEvn.up =  rotation * curEvn.up;
        curEvn.right = rotation * curEvn.right;
        _stringBuilder.AppendLine($"Roll({angle}):"+curEvn.ToString());
    }

    protected void PushEnv()
    {
        LsystemEnv env = new LsystemEnv(curEvn);
        _stack.Push(env);
        _stringBuilder.AppendLine("PushEnv:"+curEvn.ToString());
    }

    protected void PopEvn()
    {
        curEvn = _stack.Pop();
        _stringBuilder.AppendLine("PopEvn:"+curEvn.ToString());
    }
    
    protected void DivideLength(ShapeSetting shapeSetting)
    {
        curEvn.lengthScale /= shapeSetting.lengthFactor;
    }

    protected void MultipleLength(ShapeSetting shapeSetting)
    {
        curEvn.lengthScale *= shapeSetting.lengthFactor;
    }
    
    protected void IncrementWidth(ShapeSetting shapeSetting)
    {
        curEvn.widthIncrement += shapeSetting.size.x*shapeSetting.widthIncrementFactor;
    }

    protected void DecrementWidth(ShapeSetting shapeSetting)
    {
        curEvn.widthIncrement -= shapeSetting.size.x*shapeSetting.widthIncrementFactor;
    }

    protected void StartSaveSubsequentPos()
    {
        curEvn.startSavePos = true;
        curEvn.subMeshData = new SubMeshData();
        _stringBuilder.AppendLine("StartSaveSubsequentPos:"+curEvn.ToString());
    }

    protected void FillSavedPolygon()
    {
        curEvn.startSavePos = false;
        generateMeshData.subMeshDatas.Add(curEvn.subMeshData);
        curEvn.subMeshData = null;
        _stringBuilder.AppendLine("FillSavedPolygon:"+curEvn.ToString());
    }

    public void SaveFile(ShapeSetting shapeSetting)
    {
        File.WriteAllText(shapeSetting.name+"_"+shapeSetting.generateType.ToString()+"_"+shapeSetting.maxIter.ToString()+".txt",_stringBuilder.ToString());
    }
}
