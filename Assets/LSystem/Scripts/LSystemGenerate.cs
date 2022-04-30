using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LSystem.Scripts;
using UnityEngine;

// https://www.bilibili.com/read/cv3006796
public class LSystemGenerate
{

    private IGenerateImp iGenerateImp; 
    public List<Vector3> Generate(ShapeSetting shapeSetting)
    {
        iGenerateImp = CreateGenerate(shapeSetting.generateType);
        var vector3s = iGenerateImp.Generate(shapeSetting);
        iGenerateImp.SaveFile(shapeSetting);
        return vector3s;
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

    public LsystemEnv(LsystemEnv curEvn)
    {
        up = curEvn.up;
        right = curEvn.right;
        pos = curEvn.pos;
    }

    public LsystemEnv()
    {
        
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

    private LsystemEnv curEvn = new LsystemEnv();
    private Stack<LsystemEnv> _stack = new Stack<LsystemEnv>();
    private StringBuilder _stringBuilder = new StringBuilder();
    public abstract List<Vector3>  Generate(ShapeSetting shapeSetting);
    
    protected  void UpdatePos(Vector2 shapeSettingSize)
    {
        curEvn.pos += curEvn.up * shapeSettingSize.y;
        _stringBuilder.AppendLine("UpdatePos:"+curEvn.ToString());
    }

    protected void UpdateRect(Vector2 shapeSettingSize)
    {
        rect[0] = curEvn.pos + curEvn.right*shapeSettingSize.x*(-0.5f)+curEvn.up*0;
        rect[1] = curEvn.pos + curEvn.right*shapeSettingSize.x*0.5f+curEvn.up*0;
        rect[2] = curEvn.pos + curEvn.right*shapeSettingSize.x*0.5f+curEvn.up*shapeSettingSize.y;
        rect[3] = curEvn.pos + curEvn.right*shapeSettingSize.x*(-0.5f)+curEvn.up*shapeSettingSize.y;
        _stringBuilder.AppendLine("UpdateRect:"+curEvn.ToString());

    }

    protected  void AddCell(ref List<Vector3> vector3s)
    {
        vector3s.Add(rect[0]);
        vector3s.Add(rect[3]);
        vector3s.Add(rect[2]);
        
        vector3s.Add(rect[0]);
        vector3s.Add(rect[2]);
        vector3s.Add(rect[1]);
        _stringBuilder.AppendLine("AddCell:"+curEvn.ToString());
    }

    protected  void Rotation(float angle)
    {
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        curEvn.up =  rotation * curEvn.up;
        curEvn.right = rotation * curEvn.right;
        _stringBuilder.AppendLine("Rotation:"+curEvn.ToString());
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

    public void SaveFile(ShapeSetting shapeSetting)
    {
        File.WriteAllText(shapeSetting.generateType.ToString()+"_"+shapeSetting.maxIter.ToString()+".txt",_stringBuilder.ToString());
    }
}
