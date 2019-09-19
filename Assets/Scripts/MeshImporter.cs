using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshImporter : MonoBehaviour {

    List<Vector3> vertList;
    Vector2[] newUV;
    int[] newTriangles;
    Material hatchMat, pencilMat;
    DirectoryInfo[] dirs;
    int currentIdx = 0, remainShot = 0, remainMesh = 0;
    GameObject meshObj = null, camObj;
    Vector3 avg, sigma, sigma2;
    private Texture2D renderImage;

    void updateObj(GameObject obj)
    {
        vertList = new List<Vector3>();
        MeshFilter mfilter = obj.GetComponent<MeshFilter>();
        MeshRenderer render = obj.GetComponent<MeshRenderer>();

        if(mfilter != null && render != null) // this is a gameobject with mesh and renderer
        {
            Mesh mesh = mfilter.mesh;
            vertList.AddRange(mesh.vertices);

            // update mesh
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
            //  mesh.RecalculateTangents();

            // switch the texture
            bool flag = mesh.uv.Length > 0 && mesh.uv[0].magnitude > 0 && mesh.uv[mesh.uv.Length / 2].magnitude > 0 && mesh.uv[mesh.uv.Length / 3].magnitude > 0;
            Material[] materials = render.materials;
            //for (int i = 0; i < materials.Length; i++)
            //    materials[i] = (flag ? hatchMat : pencilMat);
            for (int i = 0; i < materials.Length; i++)
                materials[i] = hatchMat;
            render.materials = materials;
        }

        for (int i = 0; i < obj.transform.childCount; i++)
            updateObj(obj.transform.GetChild(i).gameObject);
    }

    // Use this for initialization
    void Start () {

        hatchMat = new Material(Shader.Find("NPR/Pencil Sketch/Hatching"));
        hatchMat.SetTexture("_Hatch5", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_5.jpg", typeof(Texture2D)));
        hatchMat.SetTexture("_Hatch4", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_4.jpg", typeof(Texture2D)));
        hatchMat.SetTexture("_Hatch3", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_3.jpg", typeof(Texture2D)));
        hatchMat.SetTexture("_Hatch2", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_2.jpg", typeof(Texture2D)));
        hatchMat.SetTexture("_Hatch1", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_1.jpg", typeof(Texture2D)));
        hatchMat.SetTexture("_Hatch0", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_0.jpg", typeof(Texture2D)));
        hatchMat.SetFloat("_TileFactor", 10);
        hatchMat.SetFloat("_Outline", 0.002f);

        pencilMat = new Material(Shader.Find("NPR/Pencil Sketch/Pencil Sketch Shading"));
        pencilMat.SetTexture("_Level1", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_5.jpg", typeof(Texture2D)));
        pencilMat.SetTexture("_Level2", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_4.jpg", typeof(Texture2D)));
        pencilMat.SetTexture("_Level3", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_3.jpg", typeof(Texture2D)));
        pencilMat.SetTexture("_Level4", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_2.jpg", typeof(Texture2D)));
        pencilMat.SetTexture("_Level5", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_1.jpg", typeof(Texture2D)));
        pencilMat.SetTexture("_Level6", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Hatch/hatch_0.jpg", typeof(Texture2D)));
        pencilMat.SetFloat("_TileFactor", 10);
        pencilMat.SetFloat("_Outline", 0.001f);

        string fullPath = "Assets/Models/Shapenet/airplane/"; 
        
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            dirs = direction.GetDirectories("*", SearchOption.TopDirectoryOnly);

            Debug.Log("Dir length: " + dirs.Length);
            Debug.Log(dirs[0].Name + " " + dirs[0].FullName);
        }

        //Screen.SetResolution(512, 512, false);
        renderImage = new Texture2D(512, 512);
    }

    // Update is called once per frame
    void Update()
    {
        if(remainMesh > 0 || remainShot > 0)
        {
            if(remainShot == 0)
            {
                remainMesh--;
                if (meshObj != null)
                    Destroy(meshObj);
                Debug.Log("New object: " + dirs[currentIdx].Name);
                meshObj = (GameObject)AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Shapenet/airplane/" + dirs[currentIdx++].Name + "/model.obj")[0];
                meshObj = Instantiate(meshObj);
                meshObj.name = "shapenet Mesh";

                updateObj(meshObj);

                avg = new Vector3(0, 0, 0);
                sigma = new Vector3(0, 0, 0);
                Vector3 c, temp;
                for (int i = 0; i < vertList.Count; i++)
                    avg += vertList[i];

                avg /= vertList.Count;
                for (int i = 0; i < vertList.Count; i++)
                {
                    c = vertList[i] - avg;
                    sigma += new Vector3(c[0] * c[0], c[1] * c[1], c[2] * c[2]);
                }
                sigma = Vector3.Normalize(sigma);
                hatchMat.SetVector("_U", new Vector4(sigma[0], sigma[1], sigma[2], 0.0f));
                if (sigma[0] < sigma[1] && sigma[0] < sigma[2]) temp = new Vector3(1, 0, 0);
                else if (sigma[1] < sigma[2]) temp = new Vector3(0, 1, 0);
                else temp = new Vector3(0, 0, 1);
                sigma2 = Vector3.Normalize(Vector3.Cross(temp, sigma));
                hatchMat.SetVector("_V", new Vector4(sigma2[0], sigma2[1], sigma2[2], 0.0f));
                remainShot = 24;
            }
            camObj = GameObject.Find("Main Camera");
            Vector3 camRot = camObj.transform.localEulerAngles;
            camRot.y += Random.Range(0, 60);
            camRot.x = Random.Range(15, 60);
            camObj.transform.localEulerAngles = camRot;
            //Debug.Log(camRot);
            //Debug.Log(camObj.transform.localPosition);

            //camRot = camObj.transform.localRotation.to
            Vector3 camPos = new Vector3((float)(-System.Math.Sin(camRot[1] / 180.0 * 3.14) * System.Math.Cos(camRot[0] / 180.0 * 3.14)),
                                        (float)(System.Math.Sin(camRot[0] / 180.0 * 3.14)),
                                        (float)(-System.Math.Cos(camRot[1] / 180.0 * 3.14) * System.Math.Cos(camRot[0] / 180.0 * 3.14))) * Random.Range(0.8f, 2);
            camObj.transform.SetPositionAndRotation(camPos + new Vector3(avg.x, 0.0f, avg.z), camObj.transform.rotation);
            //camObj.transform.localPosition = new Vector3()

            string outputDirName = "D://USC/NPR_data/airplane/" + dirs[currentIdx].Name;
            if (!Directory.Exists(outputDirName))
                Directory.CreateDirectory(outputDirName);
            StartCoroutine(SaveImage(outputDirName, 24-remainShot));
            remainShot--;
        }

        if (Input.GetKey(KeyCode.N))
        {
            if (meshObj != null)
                Destroy(meshObj);
            //Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Models/Shapenet/model.obj");
            //Debug.Log(mesh.vertices.Length/3);
            //Debug.Log(mesh.triangles.Length/3);

            //GameObject gameObj = (GameObject)AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Shapenet/airplane/1b7ac690067010e26b7bd17e458d0dcb/model.obj")[0];
            meshObj = (GameObject)AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Shapenet/airplane/" + dirs[currentIdx++].Name + "/model.obj")[0];
            //meshObj = (GameObject)AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Shapenet/model.obj")[0];
            meshObj = Instantiate(meshObj);
            meshObj.name = "shapenet Mesh";
            
            updateObj(meshObj);

            avg = new Vector3(0, 0, 0);
            sigma = new Vector3(0, 0, 0);
            Vector3 c, temp;
            for (int i = 0; i < vertList.Count; i++)
                avg += vertList[i];

            avg /= vertList.Count;
            for (int i = 0; i < vertList.Count; i++)
            {
                c = vertList[i] - avg;
                sigma += new Vector3(c[0] * c[0], c[1] * c[1], c[2] * c[2]);
            }
            sigma = Vector3.Normalize(sigma);
            hatchMat.SetVector("_U", new Vector4(sigma[0], sigma[1], sigma[2], 0.0f));
            if (sigma[0] < sigma[1] && sigma[0] < sigma[2]) temp = new Vector3(1, 0, 0);
            else if (sigma[1] < sigma[2]) temp = new Vector3(0, 1, 0);
            else temp = new Vector3(0, 0, 1);
            sigma2 = Vector3.Normalize(Vector3.Cross(temp, sigma));
            hatchMat.SetVector("_V", new Vector4(sigma2[0], sigma2[1], sigma2[2], 0.0f));
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            camObj = GameObject.Find("Main Camera");
            Vector3 camRot = camObj.transform.localEulerAngles;
            camRot.y += 30;
            camObj.transform.localEulerAngles = camRot;
            //Debug.Log(camRot);
            //Debug.Log(camObj.transform.localPosition);

            //camRot = camObj.transform.localRotation.to
            Vector3 camPos = new Vector3((float)(-System.Math.Sin(camRot[1] / 180.0 * 3.14) * System.Math.Cos(camRot[0] / 180.0 * 3.14)),
                                        (float)(System.Math.Sin(camRot[0] / 180.0 * 3.14)),
                                        (float)(-System.Math.Cos(camRot[1] / 180.0 * 3.14) * System.Math.Cos(camRot[0] / 180.0 * 3.14))) * 1.0f;
            camObj.transform.SetPositionAndRotation(camPos+new Vector3(avg.x, 0.0f, avg.z), camObj.transform.rotation);
            //camObj.transform.localPosition = new Vector3()

            string outputDirName = "Assets/Output/airplane/" + dirs[currentIdx].Name;
            if (!Directory.Exists(outputDirName))
                Directory.CreateDirectory(outputDirName);
            StartCoroutine(SaveImage(outputDirName));
        }
        if (Input.GetKeyDown(KeyCode.R)) // rendering ! 
        {
            remainMesh = dirs.Length;
            currentIdx = 0;
        }
    }

    IEnumerator SaveImage(string outputDirName, int shotIdx=0)
    {
        yield return new WaitForEndOfFrame();
        renderImage.ReadPixels(new Rect(0, 0, 512, 512), 0, 0, true);//read pixels from screen to texture
        renderImage.Apply();
        byte[] bytes = renderImage.EncodeToPNG();
        string outputFileName = outputDirName + string.Format("/{0:0000}.jpg", shotIdx);
        if (File.Exists(outputFileName))
            File.Delete(outputFileName);
        File.WriteAllBytes(outputFileName, bytes);
        Debug.Log("write a pic");
        yield return null;
    }
}
