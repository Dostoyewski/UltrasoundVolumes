using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;
using System.Globalization;

public class RobotWriter : MonoBehaviour
{
    public string Path { get; set; }
    private int Nspots = 50;
    private int Rsteps = 15;

    public void WriteSpotsFile()
    {
        var disc = GameObject.FindObjectOfType<SpotCapsule>();
        var radius = disc.GetRadius() * 35;
        var level = (float) disc.GetCurrentLevel() * 8;
        var height = disc.GetHeight();
        int iterator = 0;

        using (StreamWriter sw = new StreamWriter(Path + "/temp.txt"))
        {
            for (var j = 0; j < Rsteps; j++)
            {
                for (var i = 0; i < Nspots; i++)
                {
                    var angle = i * 2 * Math.PI / Nspots;
                    string line = iterator.ToString() + " 01.30 07.00 " + 
                                  (radius / Rsteps * (j + 1) * Math.Cos(angle)).ToString(CultureInfo.InvariantCulture);
                    line += " " + (radius / Rsteps * (j + 1) * Math.Sin(angle)).ToString(CultureInfo.InvariantCulture) + " " + 
                            level.ToString(CultureInfo.InvariantCulture) + " 100";
                    sw.WriteLine(line);
                    iterator++;
                }
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
