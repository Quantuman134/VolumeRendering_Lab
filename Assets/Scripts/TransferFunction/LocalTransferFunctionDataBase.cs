using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace UnityVolumeRendering
{
    public class LocalTransferFunctionDatabase
    {
        static public int seg_num = 16;//分割数量
        static public int list_num = 4;//局部传递函数数量
        public Texture3D texture = null;
        public List<TransferFunction> localTransferFunction_List = null;
        public List<TransferFunction>[] dataBase = null;

        public LocalTransferFunctionDatabase(int i)
        {
            CreateLocalTransferFunctionsDatabase();
            GetLocalTransferFunction_List(i);
            GenerateLocalTransferFuction3DTexture();
        }

        [System.Serializable]
        private struct TF1DSerialisationData
        {
            public int version;
            public List<TFColourControlPoint> colourPoints;
            public List<TFAlphaControlPoint> alphaPoints;

            public const int VERSION_ID = 1;
        }

        public List<TransferFunction> GetLocalTransferFunction_List(int i)
        {
            if (dataBase == null)
            {
                CreateLocalTransferFunctionsDatabase();
            }
            localTransferFunction_List = dataBase[i];
            return localTransferFunction_List;
        }

        public Texture3D GetTexture()
        {
            if (texture == null)
            {
                GenerateLocalTransferFuction3DTexture();
            }
            return texture;
        }


        //局部传递函数库
        public List<TransferFunction>[] CreateLocalTransferFunctionsDatabase()
        {
            List<TransferFunction>[] database = new List<TransferFunction>[list_num];

            //for experiment

            TransferFunction tf1 = new TransferFunction();
            TransferFunction tf2 = new TransferFunction();
            TransferFunction tf3 = new TransferFunction();
            TransferFunction tf4 = new TransferFunction();
            TransferFunction tf5 = new TransferFunction();
            TransferFunction tf6 = new TransferFunction();
            TransferFunction tf7 = new TransferFunction();
            TransferFunction tf8 = new TransferFunction();
            TransferFunction tf9 = new TransferFunction();
            TransferFunction tf10 = new TransferFunction();
            TransferFunction tf11 = new TransferFunction();
            TransferFunction tf12 = new TransferFunction();
            TransferFunction tf13 = new TransferFunction();
            TransferFunction tf14 = new TransferFunction();
            TransferFunction tf15 = new TransferFunction();
            TransferFunction tf16 = new TransferFunction();

            tf1.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.565f, 0.592f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.235f, new Color(0.8f, 0.435f, 0.443f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.369f, new Color(0.925f, 0.914f, 0.816f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.502f, new Color(0.725f, 0.722f, 0.671f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.627f, new Color(0.867f, 0.867f, 0.867f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.6f, 0.6f, 0.6f, 1.0f)));
            /*
            tf1.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.235f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.29f, 0.156f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.369f, 0.638f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.494f, 0.844f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.588f, 1.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(1.0f, 1.0f));
            */
            tf1.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.2f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.37f, 0.2f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.429f, 1.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(1.0f, 1.0f));
            tf2.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf2.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf3.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf3.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf4.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf4.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf5.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf5.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf6.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf6.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf7.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf7.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf8.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf8.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf9.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf9.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf9.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf9.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf10.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf10.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf11.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf11.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf12.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf12.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf13.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf13.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf14.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf14.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf15.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf15.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf16.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf16.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));

            //生成相应的颜色数组
            tf1.GenerateTexture();
            tf2.GenerateTexture();
            tf3.GenerateTexture();
            tf4.GenerateTexture();
            tf5.GenerateTexture();
            tf6.GenerateTexture();
            tf7.GenerateTexture();
            tf8.GenerateTexture();
            tf9.GenerateTexture();
            tf10.GenerateTexture();
            tf11.GenerateTexture();
            tf12.GenerateTexture();
            tf13.GenerateTexture();
            tf14.GenerateTexture();
            tf15.GenerateTexture();
            tf16.GenerateTexture();

            List<TransferFunction> list1_tf = new List<TransferFunction>();
            list1_tf.Add(tf1);
            list1_tf.Add(tf2);
            list1_tf.Add(tf3);
            list1_tf.Add(tf4);
            list1_tf.Add(tf5);
            list1_tf.Add(tf6);
            list1_tf.Add(tf7);
            list1_tf.Add(tf8);
            list1_tf.Add(tf9);
            list1_tf.Add(tf10);
            list1_tf.Add(tf11);
            list1_tf.Add(tf12);
            list1_tf.Add(tf13);
            list1_tf.Add(tf14);
            list1_tf.Add(tf15);
            list1_tf.Add(tf16);

            //List2--Vismale

            tf1 = new TransferFunction();
            tf2 = new TransferFunction();
            tf3 = new TransferFunction();
            tf4 = new TransferFunction();
            tf5 = new TransferFunction();
            tf6 = new TransferFunction();
            tf7 = new TransferFunction();
            tf8 = new TransferFunction();
            tf9 = new TransferFunction();
            tf10 = new TransferFunction();
            tf11 = new TransferFunction();
            tf12 = new TransferFunction();
            tf13 = new TransferFunction();
            tf14 = new TransferFunction();
            tf15 = new TransferFunction();
            tf16 = new TransferFunction();

            tf1.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.565f, 0.592f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.235f, new Color(0.8f, 0.435f, 0.443f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.369f, new Color(0.925f, 0.914f, 0.816f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.502f, new Color(0.725f, 0.722f, 0.671f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.627f, new Color(0.867f, 0.867f, 0.867f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.6f, 0.6f, 0.6f, 1.0f)));
            /*
            tf1.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.235f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.29f, 0.156f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.369f, 0.638f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.494f, 0.844f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.588f, 1.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(1.0f, 1.0f));
            */
            tf1.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.2f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.27f, 0.156f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.329f,1.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(1.0f, 1.0f));
            tf2.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf2.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf3.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf3.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf4.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf4.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf5.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf5.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf6.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf6.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf7.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf7.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf8.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf8.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf9.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf9.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf9.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf9.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf10.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf10.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf11.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf11.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf12.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf12.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf13.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf13.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf14.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf14.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf15.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf15.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf16.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf16.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));

            //生成相应的颜色数组
            tf1.GenerateTexture();
            tf2.GenerateTexture();
            tf3.GenerateTexture();
            tf4.GenerateTexture();
            tf5.GenerateTexture();
            tf6.GenerateTexture();
            tf7.GenerateTexture();
            tf8.GenerateTexture();
            tf9.GenerateTexture();
            tf10.GenerateTexture();
            tf11.GenerateTexture();
            tf12.GenerateTexture();
            tf13.GenerateTexture();
            tf14.GenerateTexture();
            tf15.GenerateTexture();
            tf16.GenerateTexture();

            List<TransferFunction> list2_tf = new List<TransferFunction>();
            list2_tf.Add(tf1);
            list2_tf.Add(tf2);
            list2_tf.Add(tf3);
            list2_tf.Add(tf4);
            list2_tf.Add(tf5);
            list2_tf.Add(tf6);
            list2_tf.Add(tf7);
            list2_tf.Add(tf8);
            list2_tf.Add(tf9);
            list2_tf.Add(tf10);
            list2_tf.Add(tf11);
            list2_tf.Add(tf12);
            list2_tf.Add(tf13);
            list2_tf.Add(tf14);
            list2_tf.Add(tf15);
            list2_tf.Add(tf16);

            //List3_neghip

            tf1 = new TransferFunction();
            tf2 = new TransferFunction();
            tf3 = new TransferFunction();
            tf4 = new TransferFunction();
            tf5 = new TransferFunction();
            tf6 = new TransferFunction();
            tf7 = new TransferFunction();
            tf8 = new TransferFunction();
            tf9 = new TransferFunction();
            tf10 = new TransferFunction();
            tf11 = new TransferFunction();
            tf12 = new TransferFunction();
            tf13 = new TransferFunction();
            tf14 = new TransferFunction();
            tf15 = new TransferFunction();
            tf16 = new TransferFunction();

            tf1.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.0f, 1.0f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.0f, 1.0f, 1.0f)));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf2.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf2.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf3.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf3.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf4.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf4.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf5.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf5.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf6.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf6.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf7.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf7.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf8.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf8.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf9.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 1.0f, 1.0f)));
            tf9.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 1.0f, 1.0f)));
            tf9.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf9.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf10.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf10.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf11.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf11.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf12.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.433f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.933f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf12.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.15f));
            tf13.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf13.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf14.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf14.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf15.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf15.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf16.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf16.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));

            //生成相应的颜色数组
            tf1.GenerateTexture();
            tf2.GenerateTexture();
            tf3.GenerateTexture();
            tf4.GenerateTexture();
            tf5.GenerateTexture();
            tf6.GenerateTexture();
            tf7.GenerateTexture();
            tf8.GenerateTexture();
            tf9.GenerateTexture();
            tf10.GenerateTexture();
            tf11.GenerateTexture();
            tf12.GenerateTexture();
            tf13.GenerateTexture();
            tf14.GenerateTexture();
            tf15.GenerateTexture();
            tf16.GenerateTexture();

            List<TransferFunction> list3_tf = new List<TransferFunction>();
            list3_tf.Add(tf1);
            list3_tf.Add(tf2);
            list3_tf.Add(tf3);
            list3_tf.Add(tf4);
            list3_tf.Add(tf5);
            list3_tf.Add(tf6);
            list3_tf.Add(tf7);
            list3_tf.Add(tf8);
            list3_tf.Add(tf9);
            list3_tf.Add(tf10);
            list3_tf.Add(tf11);
            list3_tf.Add(tf12);
            list3_tf.Add(tf13);
            list3_tf.Add(tf14);
            list3_tf.Add(tf15);
            list3_tf.Add(tf16);


            //List4_fuel

            tf1 = new TransferFunction();
            tf2 = new TransferFunction();
            tf3 = new TransferFunction();
            tf4 = new TransferFunction();
            tf5 = new TransferFunction();
            tf6 = new TransferFunction();
            tf7 = new TransferFunction();
            tf8 = new TransferFunction();
            tf9 = new TransferFunction();
            tf10 = new TransferFunction();
            tf11 = new TransferFunction();
            tf12 = new TransferFunction();
            tf13 = new TransferFunction();
            tf14 = new TransferFunction();
            tf15 = new TransferFunction();
            tf16 = new TransferFunction();

            tf1.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.02f, 0.381f, 0.998f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.024f, new Color(0.02f, 0.424f, 0.969f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.047f, new Color(0.02f, 0.467f, 0.940f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.071f, new Color(0.02f, 0.510f, 0.911f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.094f, new Color(0.02f, 0.546f, 0.873f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.118f, new Color(0.02f, 0.583f, 0.834f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.141f, new Color(0.02f, 0.619f, 0.796f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.165f, new Color(0.02f, 0.653f, 0.750f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.188f, new Color(0.02f, 0.686f, 0.704f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.212f, new Color(0.02f, 0.72f, 0.657f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.235f, new Color(0.02f, 0.757f, 0.604f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.259f, new Color(0.02f, 0.794f, 0.55f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.282f, new Color(0.02f, 0.831f, 0.496f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.306f, new Color(0.021f, 0.865f, 0.429f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.333f, new Color(0.023f, 0.898f, 0.361f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.357f, new Color(0.016f, 0.931f, 0.293f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.380f, new Color(0.274f, 0.953f, 0.154f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.404f, new Color(0.493f, 0.962f, 0.111f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.427f, new Color(0.644f, 0.977f, 0.047f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.451f, new Color(0.762f, 0.985f, 0.035f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.475f, new Color(0.881f, 0.992f, 0.022f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.498f, new Color(1.0f, 1.0f, 0.013f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.522f, new Color(1.0f, 0.955f, 0.079f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.545f, new Color(0.999f, 0.911f, 0.148f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.569f, new Color(0.999f, 0.866f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.592f, new Color(0.999f, 0.818f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.617f, new Color(0.999f, 0.770f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.643f, new Color(0.999f, 0.722f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.667f, new Color(0.999f, 0.673f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.690f, new Color(0.999f, 0.625f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.714f, new Color(0.999f, 0.577f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.737f, new Color(0.999f, 0.521f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.761f, new Color(0.999f, 0.465f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.784f, new Color(0.999f, 0.409f, 0.217f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.808f, new Color(0.995f, 0.332f, 0.211f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.831f, new Color(0.987f, 0.260f, 0.190f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.855f, new Color(0.991f, 0.148f, 0.210f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.878f, new Color(0.950f, 0.117f, 0.253f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.902f, new Color(0.903f, 0.078f, 0.292f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.925f, new Color(0.857f, 0.04f, 0.331f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.949f, new Color(0.799f, 0.043f, 0.358f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(0.976f, new Color(0.741f, 0.047f, 0.386f, 1.0f)));
            tf1.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.684f, 0.05f, 0.414f, 1.0f)));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.067f, 0.138f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.212f, 0.256f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.365f, 0.425f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.584f, 0.456f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.663f, 0.625f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.757f, 0.794f));
            tf1.AddControlPoint(new TFAlphaControlPoint(0.867f, 0.931f));
            tf1.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf2.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf2.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf2.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf3.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf3.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf3.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf4.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf4.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf4.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf5.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf5.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf5.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf6.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf6.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf6.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf7.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 0.0f, 1.0f)));
            tf7.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf7.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf8.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f)));
            tf8.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf8.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf9.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 1.0f, 1.0f)));
            tf9.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 1.0f, 1.0f)));
            tf9.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf9.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf10.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 1.0f, 0.0f, 1.0f)));
            tf10.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf10.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf11.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf11.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf11.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf12.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.0f, 0.433f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.0f, 0.933f, 0.0f, 1.0f)));
            tf12.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf12.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.15f));
            tf13.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf13.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf13.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.3f));
            tf14.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf14.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf14.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf15.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            tf15.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf15.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));
            tf16.AddControlPoint(new TFColourControlPoint(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFColourControlPoint(1.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            tf16.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf16.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.009f));

            //生成相应的颜色数组
            tf1.GenerateTexture();
            tf2.GenerateTexture();
            tf3.GenerateTexture();
            tf4.GenerateTexture();
            tf5.GenerateTexture();
            tf6.GenerateTexture();
            tf7.GenerateTexture();
            tf8.GenerateTexture();
            tf9.GenerateTexture();
            tf10.GenerateTexture();
            tf11.GenerateTexture();
            tf12.GenerateTexture();
            tf13.GenerateTexture();
            tf14.GenerateTexture();
            tf15.GenerateTexture();
            tf16.GenerateTexture();

            List<TransferFunction> list4_tf = new List<TransferFunction>();
            list4_tf.Add(tf1);
            list4_tf.Add(tf2);
            list4_tf.Add(tf3);
            list4_tf.Add(tf4);
            list4_tf.Add(tf5);
            list4_tf.Add(tf6);
            list4_tf.Add(tf7);
            list4_tf.Add(tf8);
            list4_tf.Add(tf9);
            list4_tf.Add(tf10);
            list4_tf.Add(tf11);
            list4_tf.Add(tf12);
            list4_tf.Add(tf13);
            list4_tf.Add(tf14);
            list4_tf.Add(tf15);
            list4_tf.Add(tf16);

            database[0] = list1_tf;
            database[1] = list2_tf;
            database[2] = list3_tf;
            database[3] = list4_tf;

            dataBase = database;

            return database;
        }

        //生成3D局部传递函数纹理
        private Texture3D GenerateLocalTransferFuction3DTexture()
        {
            int dimX, dimY, dimZ;
            dimZ = seg_num;
            int[] textureSize = new int[2];
            textureSize = TransferFunction.GetTextureSize();
            dimX = textureSize[1];
            dimY = textureSize[0];


            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[dimX * dimY * dimZ];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        Color[] colorsData = localTransferFunction_List[z].GetColors();
                        cols[iData] = colorsData[x + y * dimX];
                    }
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }

    }
}
