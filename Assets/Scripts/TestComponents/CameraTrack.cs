using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CameraTrack : MonoBehaviour
{
    public GameObject camera;
    public Vector3 cameraPos;

    private List<Vector3> posList = new List<Vector3>();

    public bool trackFunction = false;

    private string fileName ;

    // Start is called before the first frame update
    void Start()
    {
        fileName = "C:/Users/admin/Desktop/Project/temp/Clip.csv";

        if (!camera)
        {
            Debug.Log("No camera tracked!");
        }
        cameraPos.x = 0;
        cameraPos.y = 0;
        cameraPos.z = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (trackFunction)
        {
            cameraPos = camera.GetComponent<Transform>().position;
            Debug.Log(cameraPos);
            posList.Add(cameraPos);
        }
    }

    void OnDestroy()
    {
        ResponseExportCSV(fileName);
    }

    public void ResponseExportCSV(string fileName)
    {
        if (fileName.Length > 0)
        {
            /*
            这个地方填你需要写入的数据，数据可以从数据库等地方来
            例如：
            List<double[]> dataList= new List<double[]>();
            dataList=Db.GetData();
            */
            //这个地方是打开文件 fileName是你要创建的CSV文件的路径 例如你给个窗口选择的文件 C:/test.csv
            FileStream fs = new FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            /*
            string dataHeard = string.Empty;
            //这个地方是写入CSV的标题栏 注意最后个没有分隔符
            dataHeard = "X,Y,Z";
            sw.WriteLine(dataHeard);
            */
            //写入数据
            for (int i = 0; i < posList.Count; i++)
            {
                
                string dataStr = string.Empty;
                dataStr += posList[i].x.ToString();
                dataStr += ",";
                dataStr += posList[i].y.ToString();
                dataStr += ",";
                dataStr += posList[i].z.ToString();
                dataStr += ",";
                sw.WriteLine(dataStr);
            }
            sw.Close();
            fs.Close();

        }
    }
}


          
