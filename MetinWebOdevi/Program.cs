using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using net.zemberek.erisim;
using net.zemberek.yapi;
using net.zemberek.tr.yapi;
namespace MetinWebOdevi
{

    class Program
    {
        public static ArrayList stopWords;
        public static void StopWordsInput() //stopwords.txt dosyasındaki kelimeleri alıp stopWords listesine atar.
        {
            Console.Write("*Stop Words Listesi Dışarıdan Alınıyor...");
            stopWords = new ArrayList();
            string currentDirectory = Directory.GetCurrentDirectory();
            string dosyaYolu = System.IO.Path.Combine(currentDirectory, "stopwords.txt");
            StreamReader srr = new StreamReader(dosyaYolu, Encoding.Default);
            var fs = new FileStream(dosyaYolu, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs, Encoding.Default);

            string line = String.Empty;

            while ((line = sr.ReadLine()) != null)
            {
                stopWords.Add(line);
            }
            Console.WriteLine("(Tamamlandı!)");
        }
        public static ArrayList KayitOku(string klasorIsmi,string classIsmi)//verilen txt dosyalarını satır satır bir arrayliste ekler ve arraylist döndürür.
        {
            Console.Write("*\""+klasorIsmi+"\""+ " klasöründen " + classIsmi+" kayitlar okunuyor...");
            ArrayList tweetler = new ArrayList();
            string currentDirectory = Directory.GetCurrentDirectory();
            string dosyaYolu = System.IO.Path.Combine(currentDirectory, "raw_texts\\" + klasorIsmi + "\\");
            var TextDosyalari = Directory.EnumerateFiles(dosyaYolu, "*.txt");
            foreach (string i in TextDosyalari)
            {
                StreamReader sr = new StreamReader(i, Encoding.Default);
                tweetler.Add(sr.ReadLine() + " ~~" + classIsmi + "~~");
            }
            Console.WriteLine("(Tamamlandı!)");
            return tweetler;
        }
        public static ArrayList Birlestir(params ArrayList[] listeler)//verilen arraylistleri birleştirir.
        {
            Console.Write("*Kayit listeleri birlestiriliyor...");
            ArrayList birlesmisListe = new ArrayList();
            foreach (ArrayList list in listeler)
            {
                foreach (string i in list)
                {
                    birlesmisListe.Add(i);
                }
            }
            Console.WriteLine("(Tamamlandı!)");
            return birlesmisListe;
        }
        public static List<String[]> Tokenizer(ArrayList liste)//cümleleri kelimelere böler.
        {
            Console.Write("*Tweetler kelimelere bölünüyor...");
            char[] seperator = { ',', ' ', '.', '\'', '\"','1', '2', '3', '4', '5', '6', '7', '8', '9', '0','\\','=','-','_','#','%','*'};
            List<String[]> tokenizeListe = new List<String[]>();
            foreach (string cumle in liste)
            {
                if (cumle != null)
                {
                    string[] kelimeler = cumle.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < kelimeler.Length; i++)
                    {
                        kelimeler[i] = kelimeler[i].ToLower();
                    }
                    tokenizeListe.Add(kelimeler);
                }
            }
            Console.WriteLine("(Tamamlandı!)");
            return tokenizeListe;
        }
        public static List<List<string>> StopWordsSil(List<String[]> liste)//stopwords listesinde bulunan kelimeleri kayıtlardan siler
        {
            Console.Write("*Stop words siliniyor...");
            List<List<string>> stopWordLuList = new List<List<string>>();
            for (int i = 0; i < liste.Count; i++)
            {
                List<string> arrayList = new List<string>();
                for (int j = 0; j < liste[i].Length; j++)
                {
                    if (!stopWords.Contains(liste[i][j]))
                    {
                        arrayList.Add(liste[i][j]);
                    }
                }
                stopWordLuList.Add(arrayList);
            }
            Console.WriteLine("(Tamamlandı!)");
            return stopWordLuList;
        }
        public static void KoklereAyir(List<List<string>> liste) // kayıtları kelimelere göre köklerine ayırır
        {
            Console.Write("*Zemberek uygulamasi yardimiyla kelimeler koklerine ayriliyor...");
            Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());
            for (int i = 0; i < liste.Count; i++)
            {
                for (int j = 0; j < liste[i].Count-1; j++)
                {
                    if (zemberek.kelimeDenetle(liste[i][j].ToString()))
                    {
                        liste[i][j] = zemberek.kelimeCozumle(liste[i][j].ToString())[0].kok().icerik();
                    }
                }
            }
            Console.WriteLine("(Tamamlandı!)");
        }
        public static List<string> OzellikleriBul(List<List<string>> liste)// Bu metod iki iş yapar kayıtların köklere indirilmiş halini,attributes olarak bir 2d dizinin 0.satirina atar.Ve 3'den az geçen veya 50'den fazla geçen kelimeleri kayıtlardan siler.
        {
            Console.Write("*Attributeslar seçiliyor...");
            List<string> Ozellikler = new List<string>();

            List<string> gecenKelimeler = new List<string>();
            List<int> kelimelerinGecmeSayisi = new List<int>();
            for (int i = 0; i < liste.Count; i++)
            {
                for (int j = 0; j < liste[i].Count - 1; j++)
                {
                    if (!Ozellikler.Contains(liste[i][j]))
                    {
                        Ozellikler.Add(liste[i][j]);
                        gecenKelimeler.Add(liste[i][j]);
                        kelimelerinGecmeSayisi.Add(1);
                    }
                    else
                    {
                        int gecenIndex = gecenKelimeler.IndexOf(liste[i][j]);
                        kelimelerinGecmeSayisi[gecenIndex] += 1;
                    }
                }
            }
            for (int i = 0; i < gecenKelimeler.Count; i++)
            {
                if (kelimelerinGecmeSayisi[i] <= 3 || kelimelerinGecmeSayisi[i] > 200)
                {
                    Ozellikler.Remove(gecenKelimeler[i]);
                }

            }
            Console.WriteLine("(Tamamlandı!)");
            return Ozellikler;
        }
        public static List<List<string>> HamFrekanslariBul(List<string> ozellikler, List<List<string>> kayitlar)
        {
            List<string> hamOzellikler = new List<string>();
            List<List<string>> hamMatrix = new List<List<string>>();
            for (int i = 0; i < ozellikler.Count; i++)
            {
                hamOzellikler.Add(ozellikler[i]);
            }
            hamMatrix.Add(hamOzellikler);
            for (int i = 0; i < kayitlar.Count; i++)
            {
                List<int> frekanslar = new List<int>();
                List<string> frekanslarString = new List<string>();
                for (int j = 0; j < hamOzellikler.Count - 1; j++)
                {
                    frekanslar.Add(0);
                }
                for (int j = 0; j < kayitlar[i].Count - 1; j++)
                {
                    if (hamOzellikler.Contains(kayitlar[i][j]))
                    {
                        frekanslar[hamOzellikler.IndexOf(kayitlar[i][j])] += 1;
                    }
                }
                for (int j = 0; j < frekanslar.Count; j++)
                {
                    frekanslarString.Add(frekanslar[j].ToString());
                }
                frekanslarString.Add(kayitlar[i][kayitlar[i].Count - 1]);
                hamMatrix.Add(frekanslarString);
            }
            return hamMatrix;
        }
        public static List<List<string>> TFBul(List<List<string>> hamListe) // hamfrekans matrixinden TF hesabı yapar
        {
            List<List<string>> TFListe = new List<List<string>>();
            TFListe = hamListe.ToList();
            for (int i = 1; i < TFListe.Count; i++)
            {
                for (int j = 0; j < TFListe[i].Count - 1; j++)
                {
                    if (!TFListe[i][j].Equals("0"))
                    {
                        double cumledekiKelimeSayisi = 0;
                        for (int z = 0; z < TFListe[i].Count - 1; z++)
                        {
                            if (!TFListe[i][z].Equals("0"))
                            {
                                cumledekiKelimeSayisi += Convert.ToDouble(TFListe[i][z]);
                            }
                        }
                        TFListe[i][j] = (Convert.ToDouble(TFListe[i][j]) / cumledekiKelimeSayisi).ToString();
                    }
                }
            }
            return TFListe;
        }
        public static List<double> IdfBul(List<List<string>> hamFrekansMatrix) // ham frekans matrixinden idf hesabı yapar.
        {
            List<double> idfListesi = new List<double>();
            for (int i = 0; i < hamFrekansMatrix[0].Count - 1; i++)
            {
                double gectigiBelgeSayisi = 0.0;
                for (int j = 1; j < hamFrekansMatrix.Count; j++)
                {
                    if (!hamFrekansMatrix[j][i].Equals("0"))
                    {
                        gectigiBelgeSayisi += 1;
                    }
                }
                idfListesi.Add(Math.Log10((hamFrekansMatrix.Count - 1) / gectigiBelgeSayisi));
            }
            return idfListesi;
        }
        public static List<List<string>> TFIdfIslemleri(List<string>ozellikler,List<List<string>> kayitlar)//verilen attributes ve kayıtlara tfidf işlemleri uygulayıp geriye tf-idf matrixi döndürür
        {
            List<List<string>> hamFrekansMatrix = HamFrekanslariBul(ozellikler, kayitlar);
            List<List<string>> tfMatrix = TFBul(hamFrekansMatrix);
            List<double> idfListesi = IdfBul(hamFrekansMatrix);
            List<List<string>> TFIdfMatrix = tfMatrix.ToList();
            for (int i = 0; i < TFIdfMatrix[0].Count-1; i++)
            {
                for (int j = 1; j < TFIdfMatrix.Count; j++)
                {
                    TFIdfMatrix[j][i] = (Convert.ToDouble(TFIdfMatrix[j][i]) * idfListesi[i]).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return TFIdfMatrix;
        }
        
        public static Dictionary<string,int> ClassFrekanslariBul(List<List<string>> kayitlar)
        {
            int olumluClassFrekansi = 0;
            int olumsuzClassFrekansi = 0;
            int notrClassFrekansi = 0;
            Dictionary<string,int> dict = new Dictionary<string,int>();
            for (int i = 0; i < kayitlar.Count; i++)
            {
                if (kayitlar[i].Contains("~~olumlu~~"))
                {
                    olumluClassFrekansi += 1;
                }
                else if (kayitlar[i].Contains("~~olumsuz~~"))
                {
                    olumsuzClassFrekansi += 1;
                }
                else if (kayitlar[i].Contains("~~notr~~"))
                {
                    notrClassFrekansi += 1;
                }
            }
            dict.Add("olumluClassFrekansi", olumluClassFrekansi);
            dict.Add("olumsuzClassFrekansi", olumsuzClassFrekansi);
            dict.Add("notrClassFrekansi", notrClassFrekansi);
            return dict;
        }
        public static void MutualInformation(List<string> ozellikler, List<List<string>> kayitlar) //mutual information score'u 0.0005den küçük olan özellikleri siler.
        {
            Console.Write("*Mutual Information Score degerleri 0.0005'den kucuk olan attributeslar siliniyor...");
            List<double> miScore = new List<double>();
            Dictionary<string, int> dict = ClassFrekanslariBul(kayitlar);
            int olumluClassFrekansi = dict["olumluClassFrekansi"];
            int olumsuzClassFrekansi = dict["olumsuzClassFrekansi"];
            int notrClassFrekansi = dict["notrClassFrekansi"];
            int toplamFrekans = olumluClassFrekansi + olumsuzClassFrekansi + notrClassFrekansi;
            for (int i = 0; i < ozellikler.Count-1; i++)
            {
                double olumludaGecen = 0;
                double olumludaGecmeyen = 0;
                double olumsuzdaGecen = 0;
                double olumsuzdaGecmeyen = 0;
                double notrdeGecen = 0;
                double notrdeGecmeyen = 0;
                for (int j = 0; j < kayitlar.Count; j++)
                {
                    if (kayitlar[j].Contains(ozellikler[i]))
                    {
                        if (kayitlar[j].Contains("~~olumlu~~"))
                        {
                            olumludaGecen += 1;
                        }
                        else if (kayitlar[j].Contains("~~olumsuz~~"))
                        {
                            olumsuzdaGecen += 1;
                        }
                        else if (kayitlar[j].Contains("~~notr~~"))
                        {
                            notrdeGecen += 1;
                        }
                    }
                }
                double toplamGecen = olumludaGecen + olumsuzdaGecen + notrdeGecen;
                olumludaGecmeyen = olumluClassFrekansi - olumludaGecen;
                olumsuzdaGecmeyen = olumsuzClassFrekansi - olumsuzdaGecen;
                notrdeGecmeyen = notrClassFrekansi - notrdeGecen;
                double toplamGecmeyen = toplamFrekans - toplamGecen;
                double MutualInfoPuani = 0;
                MutualInfoPuani += ((olumludaGecen / toplamFrekans) * (Math.Log((olumludaGecen * toplamFrekans / (olumluClassFrekansi * toplamGecen)), 2.0))).Equals(double.NaN) ? 0: ((olumludaGecen / toplamFrekans) * (Math.Log((olumludaGecen * toplamFrekans / (olumluClassFrekansi * toplamGecen)), 2.0)));
                MutualInfoPuani += ((olumsuzdaGecen / toplamFrekans) * (Math.Log((olumsuzdaGecen * toplamFrekans / (olumsuzClassFrekansi * toplamGecen)), 2.0))).Equals(double.NaN) ? 0: ((olumsuzdaGecen / toplamFrekans) * (Math.Log((olumsuzdaGecen * toplamFrekans / (olumsuzClassFrekansi * toplamGecen)), 2.0)));
                MutualInfoPuani += ((notrdeGecen/ toplamFrekans) * (Math.Log((notrdeGecen*toplamFrekans/(notrClassFrekansi*toplamGecen)),2.0))).Equals(double.NaN) ? 0: ((notrdeGecen / toplamFrekans) * (Math.Log((notrdeGecen * toplamFrekans / (notrClassFrekansi * toplamGecen)), 2.0)));
                MutualInfoPuani += ((olumludaGecmeyen / toplamFrekans) * (Math.Log((olumludaGecmeyen * toplamFrekans / (olumluClassFrekansi * toplamGecmeyen)),2.0))).Equals(double.NaN) ? 0: ((olumludaGecmeyen / toplamFrekans) * (Math.Log((olumludaGecmeyen * toplamFrekans / (olumluClassFrekansi * toplamGecmeyen)), 2.0)));
                MutualInfoPuani += ((olumsuzdaGecmeyen / toplamFrekans) * (Math.Log((olumsuzdaGecmeyen * toplamFrekans / (olumsuzClassFrekansi * toplamGecmeyen)), 2.0))).Equals(double.NaN) ? 0: ((olumsuzdaGecmeyen / toplamFrekans) * (Math.Log((olumsuzdaGecmeyen * toplamFrekans / (olumsuzClassFrekansi * toplamGecmeyen)), 2.0)));
                MutualInfoPuani += ((notrdeGecmeyen / toplamFrekans) * (Math.Log((notrdeGecmeyen * toplamFrekans / (notrClassFrekansi * toplamGecmeyen)), 2.0))).Equals(double.NaN) ? 0: ((notrdeGecmeyen / toplamFrekans) * (Math.Log((notrdeGecmeyen * toplamFrekans / (notrClassFrekansi * toplamGecmeyen)), 2.0)));
                miScore.Add(MutualInfoPuani);
            }
            for (int i = 0; i < miScore.Count; i++)
            {
                if (miScore[i] < 0.0005)
                {
                    ozellikler.Remove(ozellikler[i]);
                    miScore.Remove(miScore[i]);
                }
            }
            Console.WriteLine("(Tamamlandı!)");
        }
        public static void TfIdfYazdir(List<List<string>> liste)
        {
            Console.Write("Tf-Idf matrixi yazdiriliyor...");
            string currentDirectory = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(currentDirectory + "\\Output");
            string dosyaYolu = System.IO.Path.Combine(currentDirectory, "Output\\", "Tf-IdfMatrix.txt");
            StreamWriter sw = new StreamWriter(dosyaYolu);
            for (int i = 0; i < liste.Count; i++)
            {
                for (int j = 0; j < liste[i].Count; j++)
                {
                    sw.Write(liste[i][j]);
                    if (j != liste[i].Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.WriteLine();
            }
            Console.WriteLine(dosyaYolu + " yoluna yazdırıldı.(Tamamlandı!)");
            sw.Close();
        }

        public static void KfoldsCrossValidation(List<string> ozellikler, List<List<string>> kayitlar)
        {
            Console.WriteLine("*10-Folds tekniği ile k-NN algoritması uygulanıyor...(Cosine metric,k = 1)");
            Console.WriteLine("Not: Bu işlem bilgisayarınızın performansına göre birkaç dakika sürebilir.Arada bir klavyeden (Enter) hariç herhangi bir tuşa basmanız önerilir");
            List <List<List<string>>> parcalar = new List<List<List<string>>>();
            List<List<string>> kayitlarKopya = kayitlar.ToList();
            Random rnd = new Random();
            List<List<string>> kayit = new List<List<string>>();
            Dictionary<string, int> classFrekanslari = ClassFrekanslariBul(kayitlar);
            int olumluClassFrekansi = classFrekanslari["olumluClassFrekansi"];
            int olumsuzClassFrekansi = classFrekanslari["olumsuzClassFrekansi"];
            int notrClassFrekansi = classFrekanslari["notrClassFrekansi"];
            int toplamFrekans = olumluClassFrekansi + olumsuzClassFrekansi + notrClassFrekansi;
            for (int i = 0; i < 10; i++)
            {
                List<List<string>> none = new List<List<string>>();
                parcalar.Add(none);
            }
            int parcalarIndex = 0;
            while (kayitlarKopya.Count != 0)//Kfolds için 300'erli kayıt seçip 10 parçaya böler.Class dağılım oranları korunur.
            {
                parcalar[parcalarIndex % 10].Add(kayitlarKopya[0]);
                kayitlarKopya.Remove(kayitlarKopya[0]);
                parcalarIndex++;
            }
            double tpOlumlu = 0, fpOlumlu = 0, fnOlumlu = 0, tnOlumlu = 0, tpOlumsuz = 0, fpOlumsuz = 0, fnOlumsuz = 0, tnOlumsuz = 0, tpNotr = 0, tnNotr = 0, fpNotr = 0,fnNotr =0;
            for (int i = 0; i < parcalar.Count; i++)//10 farklı 2700 train 300 test kaydı oluşturur ve 10-folds algoritmasını çalıştırır.
            {
                List<List<string>> testKayitlari = parcalar[i].ToList();
                List<List<string>> trainKayitlari = new List<List<string>>();
                for (int j = 0; j < parcalar.Count; j++)
                {
                    if (i != j)
                    {
                        trainKayitlari = trainKayitlari.Concat(parcalar[j]).ToList();
                    }
                }
                List<List<string>> trainTfidfMatrix = TFIdfIslemleri(ozellikler, trainKayitlari);
                List<double> trainIDF = IdfBul(HamFrekanslariBul(ozellikler, trainKayitlari));
                List<List<string>> testTFMatrix = TFBul(HamFrekanslariBul(ozellikler, testKayitlari));
                List<List<string>> testTFIdfMatrix = testTFMatrix.ToList();
                for (int j = 0; j < testTFIdfMatrix[0].Count - 1; j++)
                {
                    for (int z = 1; z < testTFIdfMatrix.Count - 1; z++)
                    {
                        testTFIdfMatrix[z][j] = (Convert.ToDouble(testTFIdfMatrix[z][j]) * trainIDF[j]).ToString();
                    }
                }
                for (int j = 1; j < testTFIdfMatrix.Count; j++) //cosinus benzerliğini hesaplayıp kayıtları en yüksek benzerlikli class'a atar.(k = 1) k-NN algoritmasını uygular
                {
                    List<double> cosBenzerlikleri = new List<double>();
                    for (int z = 1; z < trainTfidfMatrix.Count; z++)
                    {
                        double nokta = 0;
                        double d1 = 0;
                        double d2 = 0;
                        for (int k = 0; k < ozellikler.Count - 1; k++)
                        {
                            if (!testTFIdfMatrix[j][k].Equals("0") && !trainTfidfMatrix[z][k].Equals("0"))
                            {
                                nokta += Convert.ToDouble(trainTfidfMatrix[z][k]) * Convert.ToDouble(testTFIdfMatrix[j][k]);
                                d1 += Convert.ToDouble(testTFIdfMatrix[j][k]) * Convert.ToDouble(testTFIdfMatrix[j][k]);
                                d2 += Convert.ToDouble(trainTfidfMatrix[z][k]) * Convert.ToDouble(trainTfidfMatrix[z][k]);
                            }
                        }
                        double cosBenzerligi = ((nokta) / (Math.Sqrt(d1) * Math.Sqrt(d2))).Equals(double.NaN) ? 0 : ((nokta) / (Math.Sqrt(d1) * Math.Sqrt(d2)));
                        
                        cosBenzerlikleri.Add(cosBenzerligi);
                    }
                    int indexNO = cosBenzerlikleri.IndexOf(cosBenzerlikleri.Max()) + 1;
                    testTFIdfMatrix[0].Add("Prediction:");
                    testTFIdfMatrix[j].Add(trainTfidfMatrix[indexNO][trainTfidfMatrix[indexNO].Count - 1]);
                }
                for (int j = 1; j < testTFIdfMatrix.Count; j++)
                {
                    if(testTFIdfMatrix[j][testTFIdfMatrix[j].Count-2].Equals("~~olumlu~~")&& testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        tpOlumlu += 1;
                    }
                    else if(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals("~~olumlu~~") && !testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        fnOlumlu += 1;
                    }
                    else if (testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1].Equals("~~olumlu~~") && !testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        fpOlumlu += 1;
                    }
                    else
                    {
                        tnOlumlu += 1;
                    }
                    if(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals("~~olumsuz~~") && testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        tpOlumsuz += 1;
                    }
                    else if (testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals("~~olumsuz~~") && !testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        fnOlumsuz += 1;
                    }
                    else if (testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1].Equals("~~olumsuz~~") && !testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        fpOlumsuz+= 1;
                    }
                    else
                    {
                        tnOlumsuz += 1;
                    }
                    if (testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals("~~notr~~") && testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        tpNotr += 1;
                    }
                    else if (testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals("~~notr~~") && !testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        fnNotr += 1;
                    }
                    else if (testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1].Equals("~~notr~~") && !testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 2].Equals(testTFIdfMatrix[j][testTFIdfMatrix[j].Count - 1]))
                    {
                        fpNotr += 1;
                    }
                    else
                    {
                        tnNotr += 1;
                    }

                }


            }
            Console.WriteLine("Test Sonuclari Yazdiriliyor...");//sonuclari yazdirir.
            List<List<string>> tfIdf = TFIdfIslemleri(ozellikler, kayitlar);
            TfIdfYazdir(tfIdf);
            double precisionOlumlu = tpOlumlu / (tpOlumlu + fpOlumlu);
            double precisionOlumsuz = tpOlumsuz / (tpOlumsuz + fpOlumsuz);
            double precisionNotr = tpNotr / (tpNotr + fpNotr);
            double recallOlumlu = tpOlumlu / (tpOlumlu + fnOlumlu);
            double recallOlumsuz = tpOlumsuz / (tpOlumsuz + fnOlumsuz);
            double recallNotr = tpNotr / (tpNotr + fnNotr);
            double fScoreOlumlu = 2 * (precisionOlumlu * recallOlumlu) / (precisionOlumlu + recallOlumlu);
            double fScoreOlumsuz = (precisionOlumsuz * recallOlumsuz) / (precisionOlumsuz + recallOlumsuz);
            double fScoreNotr = 2 * (precisionNotr * recallNotr) / (precisionNotr + recallNotr);
            double ortalamaPrecisionMacro =(precisionOlumlu + precisionNotr + precisionOlumsuz) / 3;
            double ortalamaRecallMacro = (recallOlumlu + recallOlumsuz + recallNotr) / 3;
            double ortalamaPrecisionMicro = (tpOlumlu + tpOlumsuz + tpNotr) / (tpOlumlu + fpOlumlu + tpOlumsuz + fpOlumsuz + tpNotr + fpNotr);
            double ortalamaRecallMicro = (tpOlumlu + tpOlumsuz + tpNotr) / (tpOlumlu + fnOlumlu + tpOlumsuz + fnOlumsuz + tpNotr + fnNotr);
            double fScoreMacro = 2 * (ortalamaPrecisionMacro * ortalamaRecallMacro) / (ortalamaPrecisionMacro + ortalamaRecallMacro);
            double fScoreMicro = 2 * (ortalamaPrecisionMicro * ortalamaRecallMicro) / (ortalamaPrecisionMicro + ortalamaRecallMicro);


            Console.WriteLine("Tahmin Sonuçları Yazdırılıyor...");
            string currentDirectory = Directory.GetCurrentDirectory();
            string dosyaYolu = System.IO.Path.Combine(currentDirectory, "Output\\","TahminSonuclari.txt");
            StreamWriter sw = new StreamWriter(dosyaYolu);
            sw.WriteLine("          Sınıf1(Olumlu),Sınıf2(Olumsuz),Sınıf3(Notr),Macro Average,Micro Average");
            sw.WriteLine("Precision:"+precisionOlumlu.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + precisionOlumsuz.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + precisionNotr.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + ortalamaPrecisionMacro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + ortalamaPrecisionMicro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
            sw.WriteLine("Recall:   "+recallOlumlu.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + recallOlumsuz.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + recallNotr.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + ortalamaRecallMacro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + ortalamaRecallMicro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
            sw.WriteLine("F-score:  " + fScoreOlumlu.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + fScoreOlumsuz.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + fScoreNotr.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + fScoreMacro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + fScoreMicro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
            sw.WriteLine("TP Adedi: " + tpOlumlu + "," + tpOlumsuz + "," + tpNotr);
            sw.WriteLine("FP Adedi: " + fpOlumlu + "," + fpOlumsuz + "," + fpNotr);
            sw.WriteLine("FN Adedi: " + fnOlumlu + "," + fnOlumsuz + "," + fpNotr);
            sw.WriteLine("------------------------------------------------------------------");
            sw.WriteLine("                  Sınıf1(Olumlu),Sınıf2(Olumsuz),Sınıf3(Notr) stratified 10-fold cross validation metoduna göre ortalamalar");
            sw.WriteLine("Ortalama TP Adedi: " + (tpOlumlu/10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (tpOlumsuz/10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (tpNotr/10).ToString(System.Globalization.CultureInfo.InvariantCulture));
            sw.WriteLine("Ortalama FP Adedi: " + (fpOlumlu/10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (fpOlumsuz/10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (fpNotr/10).ToString(System.Globalization.CultureInfo.InvariantCulture));
            sw.WriteLine("Ortalama FN Adedi: " + (fnOlumlu/10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (fnOlumsuz/10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (fpNotr/10).ToString(System.Globalization.CultureInfo.InvariantCulture));
            sw.Close();
            Console.Write(dosyaYolu+" yoluna yazdırıldı(Tamamlandı!)");
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Program Başladı...");
            StopWordsInput();
            ArrayList olumluTweetler = KayitOku("1", "Olumlu");
            ArrayList olumsuzTweetler = KayitOku("2", "Olumsuz");
            ArrayList notrTweetler = KayitOku("3", "Notr");
            ArrayList birlesmisListe = Birlestir(olumluTweetler, olumsuzTweetler, notrTweetler);
            List<String[]> featureTokenize = Tokenizer(birlesmisListe);
            List<List<string>> kayitlarIslenmis = StopWordsSil(featureTokenize);
            KoklereAyir(kayitlarIslenmis);
            List<string> OzellikMatrix = OzellikleriBul(kayitlarIslenmis);
            
            MutualInformation(OzellikMatrix, kayitlarIslenmis);
            KfoldsCrossValidation(OzellikMatrix, kayitlarIslenmis);
            Console.WriteLine("\nProgram başarıyla tamamlandı! Test sonuçları için yolu yazılan klasöre bakın.\n");
            Console.WriteLine("Dikkat csv dosyalarında ondalıklı değerleri 0.00 şeklinde yazdırır.");
            Console.WriteLine("Cikis için Enter Tusuna basin");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { };


        }
        
    }
}
