using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ATdecrypt
{
    class Program
    {
        static void Main(string[] args)
        { 
            string filebpath = Environment.GetEnvironmentVariable("LocalAppData") + "\\Temp\\file-b.txt"; 
            byte[] fbytes = File.ReadAllBytes(filebpath);
            List<int> fkey = findkeys(fbytes);
            int[] fkeyarray = new int[3];

            int count = fkey.Count / 3;
            for(int k = 0; k < count; k++)
            {
                Console.WriteLine(k+1 + ". " + fkey[(k)*3] + " " + fkey[(k)*3+1] + " " + fkey[(k)*3+2]);
            }

            Console.WriteLine("Fernus anahtarı bulunuyor...");
            if (fkey.Count == 0)
            {
                Console.WriteLine("Fernus anahtar şifresi bulunamadı!");
                Environment.Exit(0);
            }

            if(fkey.Count == 3)
            {
                Console.WriteLine("Anahtar: " + fkey[0] + " " + fkey[1] + " " + fkey[2]);
            }
            if(fkey.Count > 3 && fkey.Count % 3 == 0)
            {
                Console.WriteLine(fkey.Count / 3 + " adet anahtar bulundu, hangisi ile decrypt edilsin?");
                int choice = Convert.ToInt32(Console.ReadLine());
                Console.Clear();
                if (choice <= fkey.Count / 3)
                {
                    fkeyarray[0] = fkey[(choice-1)*3];
                    fkeyarray[1] = fkey[(choice-1)*3+1];
                    fkeyarray[2] = fkey[(choice-1)*3+2];
                }
            }
            else
            {
                fkeyarray[0] = fkey[0];
                fkeyarray[1] = fkey[1];
                fkeyarray[2] = fkey[2];
            }
            Console.WriteLine("Decrypt ediliyor...");
            Console.WriteLine(fkeyarray[0] + " " + fkeyarray[1] + " " + fkeyarray[2]);
            fbytes = fernusDecrypt(fbytes, fkeyarray);
            Console.WriteLine("Kaydetmek için dosya adı giriniz:");
            File.WriteAllBytes(Console.ReadLine() + ".swf", fbytes);
        }

        static byte[] fernusDecrypt(byte[] fbytes, int[] fkey)
        {
            fbytes = seperateBytes(fbytes, 10000, 11000, fkey[0], fkey[1]);
            fbytes = seperateBytes(fbytes, 5000, 5500, fkey[2], fkey[0]);
            fbytes = seperateBytes(fbytes, 850, 1500, fkey[1], fkey[2]);
            fbytes = seperateBytes(fbytes, 0, 300, fkey[0], fkey[1]);
            fbytes[fkey[2]] = (byte)(fbytes[fkey[2]] - fkey[2]);
            fbytes[fkey[1]] = (byte)(fbytes[fkey[1]] - fkey[1]);
            fbytes[fkey[0]] = (byte)(fbytes[fkey[0]] - fkey[0]);
            fbytes[2] = (byte)(fbytes[2] - fkey[2]);
            fbytes[1] = (byte)(fbytes[1] - fkey[1]);
            fbytes[0] = (byte)(fbytes[0] - fkey[0]);

            return fbytes;
        }

        static byte[] fernusEncrypt(byte[] fbytes, int[] fkey)
        {
            fbytes[0] = (byte)(fbytes[0] + fkey[0]);
            fbytes[1] = (byte)(fbytes[1] + fkey[1]);
            fbytes[2] = (byte)(fbytes[2] + fkey[2]);
            fbytes[fkey[0]] = (byte)(fbytes[fkey[0]] + fkey[0]);
            fbytes[fkey[1]] = (byte)(fbytes[fkey[1]] + fkey[1]);
            fbytes[fkey[2]] = (byte)(fbytes[fkey[2]] + fkey[2]);
            fbytes = mixBytes(fbytes, 0, 300, fkey[0], fkey[1]);
            fbytes = mixBytes(fbytes, 850, 1500, fkey[1], fkey[2]);
            fbytes = mixBytes(fbytes, 5000, 5500, fkey[2], fkey[0]);
            fbytes = mixBytes(fbytes, 10000, 11000, fkey[0], fkey[1]);
            return fbytes;
        }

        static byte[] mixBytes(byte[] fbytes, int dbyte, int ebyte, int n1, int n2) //290
        {
        	int i;
        	int size = ebyte - dbyte + n1 * 10-1;
            byte[] tbytes = new byte[size+1];
        	for(i = dbyte; i < ebyte + n1 * 10; i++)
			{   
                //Console.WriteLine(Convert.ToString(i)  + " " + fbytes[i]);
        		tbytes[i-dbyte] = (byte)((int)fbytes[i] + (byte)(n2 * 2));
        	}

            Array.Reverse(tbytes);

            for(i = dbyte; i < ebyte + n1 * 10; i++)
            {
                fbytes[i] = tbytes[i-dbyte];
            }
            return fbytes;
        }

        static byte[] seperateBytes(byte[] fbytes, int dbyte, int ebyte, int n1, int n2)
        {
        	int i;
        	int size = ebyte - dbyte + n1 * 10-1;
            List<byte> tbytes = new List<byte>();
        	for(i = dbyte; i < ebyte + n1 * 10; i++)
			{   
                //Console.WriteLine(Convert.ToString(i)  + " " + fbytes[i]);
        		tbytes.Add((byte)((int)fbytes[i] - (byte)(n2 * 2)));
        	}

            tbytes.Reverse();
            int k = 0;
            for(i = dbyte; i < ebyte + n1 * 10; i++)
            {
                
                fbytes[i] = tbytes[k];
                k++;
            }
            return fbytes;
        }

        static string keygen(int[] fkey)
        {
            
            char[] cArray = "ABCDEFGHJKLMNPRSTVYZ123456789".ToCharArray();
            int[] keys1 = {0, 3, 4, 5, 7};
            var rand = new Random();
            int keyvalue = 0;
            char[] key = new char[8];
            int random;
            foreach(int keyIndex in keys1)
            {
                random = rand.Next(cArray.Length);
                keyvalue = keyvalue + (int)cArray[random];
                key[keyIndex] = (char)cArray[random];
            }
            
            key[1] = cArray[keyvalue % fkey[0]];
            key[2] = cArray[keyvalue % fkey[1]];
            key[6] = cArray[keyvalue % fkey[2]];
            string keyString = new string(key);
            
            return keyString;
        }
        static List<int> findkeys(byte[] tbytes)
        {
            byte[] tbytes1 = new byte[900];
            Array.Copy(tbytes, 0, tbytes1, 0, 900);
            List<int> fkey = new List<int>();
            for(int i = 1; i < 55; i++)
            {
                for(int k = 0; k < 80; k++)
                {
                    for(int j = 0; j < 80; j++)
                    {
                        byte[] fbytes = new byte[tbytes1.Length]; 
                        Array.Copy(tbytes1, 0, fbytes, 0, 900);
                        fbytes = seperateBytes(fbytes, 0, 300, i, k);
                        fbytes[j] = (byte)(fbytes[j] - j);
                        fbytes[k] = (byte)(fbytes[k] - k);
                        fbytes[i] = (byte)(fbytes[i] - i);
                        fbytes[2] = (byte)(fbytes[2] - j);
                        fbytes[1] = (byte)(fbytes[1] - k);
                        fbytes[0] = (byte)(fbytes[0] - i);
                        //Console.WriteLine(i + " " + k + " " + j + " " + fbytes[0] + " " + fbytes[1] + " " + fbytes[2]);
                        if (fbytes[0] == 0x43 && fbytes[1] == 0x57 && fbytes[2] == 0x53)
                        {
                            fkey.Add(i);
                            fkey.Add(k);
                            fkey.Add(j);
                        }
                    }
                }
            }
            return fkey;
        }
    }
}

