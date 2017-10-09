using UnityEngine;
using System.Collections;
using System.Text;

public class HalHelper {



    public static string SaveDirectory;


    static HalHelper()
    {

        SaveDirectory = GetSaveDirectory() + @"/HalBlocks/";

        if (!System.IO.Directory.Exists(SaveDirectory))
            System.IO.Directory.CreateDirectory(SaveDirectory);
        SaveDirectory += "BlockFiles/";
        if (!System.IO.Directory.Exists(SaveDirectory))
            System.IO.Directory.CreateDirectory(SaveDirectory);

    }

    public static string GetSaveDirectory()
    {

        byte[] bytes = Encoding.Default.GetBytes(GameUtils.GetSaveGameDir());
        return Encoding.UTF8.GetString(bytes);
    }

    public static string GetBlockPropertyAsString(Block block, string propertyName)
    {
        if (block == null)
        {
            Debug.Log("Block for property find was null!");
            return null;
        }
        else
        {
            if (block.Properties.Values.ContainsKey(propertyName))
            {
                string str = block.Properties.Values[propertyName];
                return str;
            }
            else
            {
                Debug.Log("Block " + block.GetBlockName() + " does not have the property: " + propertyName);
                return null;
            }
        }
    }

    public static string GetBlockPropertyAsString(string blockName, string propertyName)
    {
        Block block = null;

        if (Block.list == null)
        {
            Debug.Log("LIST NULL!");
            return null;
        }
        else
        {

            foreach (Block b in Block.list)
            {
                if (b == null)
                    continue;

                if (b.GetBlockName() == blockName)
                {
                    block = b;
                    break;
                }
            }

            if (block == null)
            {
                Debug.Log("Block for property find was null!");
                return null;
            }
            else
            {


                if (block.Properties.Values.ContainsKey(propertyName))
                {
                    string str = block.Properties.Values[propertyName];
                    return str;
                }
                else
                {
                    Debug.Log("Block " + block.GetBlockName() + " does not have the property: " + propertyName);
                    return null;
                }
            }
        }
    }
    public static System.IO.BinaryWriter GetWriterForBlockFile(Vector3i pos)
    {
        return GetWriterForBlockFile(pos.x, pos.y, pos.z);
    }
    public static System.IO.BinaryWriter GetWriterForBlockFile(int x, int y, int z)
    {

        string path = SaveDirectory + x.ToString() + " " + y.ToString() + " " + z.ToString() + ".block";
        System.IO.Stream stream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);

        System.IO.BinaryWriter ret = new System.IO.BinaryWriter(stream);

        return ret;

    }
    public static System.IO.BinaryReader GetReaderForBlockFile(Vector3i pos)
    {
        return GetReaderForBlockFile(pos.x, pos.y, pos.z);
    }

    public static System.IO.BinaryReader GetReaderForBlockFile(int x, int y, int z)
    {

        string path = SaveDirectory + x.ToString() + " " + y.ToString() + " " + z.ToString() + ".block";

        if (!System.IO.File.Exists(path))
            return null;

        System.IO.Stream stream = new System.IO.FileStream(path, System.IO.FileMode.Open);
        System.IO.BinaryReader ret = new System.IO.BinaryReader(stream);
        return ret;

    }
    public static void DeleteSaveFileForBlock(Vector3i pos)
    {
        DeleteSaveFileForBlock(pos.x, pos.y, pos.z);
    }
    public static void DeleteSaveFileForBlock(int x, int y, int z)
    {
        string path = SaveDirectory + x.ToString() + " " + y.ToString() + " " + z.ToString() + ".block";
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }


}
