using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DescriptJsonFile
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {

            //quantidade de casas 
            int casas = 10;
            //retorno da requisicao da API
            var retorno = GetJsonEncript("" +
                "https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=c45b388f3d2cae71c3169e48c101133ff8f315a8").Result;


            var dataJson = DeserializeJson(retorno);


            var decifrado = DecriptLetter(casas, dataJson.Cifrado);
            dataJson.Decifrado = decifrado;
            dataJson.Resumo_criptografico = Hash(decifrado);

            DownloadJsonFile(dataJson);
            var result = PostJsonFileAsync().Result;
        }

        private static async Task<string> PostJsonFileAsync()
        {
            using (var content = new MultipartFormDataContent("----MyBoundary"))
            {
                using (FileStream s = File.Open("C:\\Users\\maira\\source\\repos\\DescriptJsonFile\\answer.json", FileMode.Open))
                using (var memoryStream = s)
                {
                    using (var stream = new StreamContent(memoryStream))
                    {
                        content.Add(stream, "answer", "answer.json");

                        using (HttpClient client = new HttpClient())
                        {
                            var responce = await client.PostAsync("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=c45b388f3d2cae71c3169e48c101133ff8f315a8", content);
                            string contents = await responce.Content.ReadAsStringAsync();
                            return contents;
                        }
                    }
                }
            }
        }

        private static void DownloadJsonFile(DataJson dataJson)
        {
            var json = SerializeJson(dataJson);
            using (StreamWriter str = new StreamWriter("C:\\Users\\maira\\source\\repos\\DescriptJsonFile\\answer.json"))
            {
                str.Write(json);
            }
        }

        private static string DecriptLetter(int casas, string cifrado)
        {
            //transforma a string em bytes
            byte[] asciiBytes = Encoding.ASCII.GetBytes(cifrado);
            //instancia um array de bytes
            byte[] asciiBytesReturned = new byte[asciiBytes.Length];

            //percorre o array de bytes
            for (int i = 0; i < asciiBytes.Length; i++)
            {

                //convert o valor do array em inteiro
                var num = Convert.ToInt32(asciiBytes[i]);


                //32 e 46 nao sao convertidos - espaco e ponto
                if (num != 32 && num != 46)
                {
                    //subtrai o numero da tabela ascii pelo numero de casas
                    var converted = num - casas;

                    //se for menor que 97 precisa voltar para o 122 ultima letra do alfabeto minusculo na ascii
                    if (converted < 97)
                    {
                        //subtrai o numero convertido pela a primeira letra tabela ascii
                        var sobra = 97 - converted - 1;
                        //subtrai o ultimo numero da tabel aascii o z com a sobra
                        var letterReturned = 122 - sobra;
                        //converte bytes
                        asciiBytesReturned[i] = Convert.ToByte(letterReturned);
                    }
                    else
                    {
                        asciiBytesReturned[i] = Convert.ToByte(converted);
                    }
                }  
                else
                {
                    asciiBytesReturned[i] = Convert.ToByte(num);
                }
            }
            //converte os bytes em string para retornar a frase criptografada
            var decifrado = Encoding.ASCII.GetString(asciiBytesReturned);
            return decifrado;
        }

        static string Hash(string input)
        {
            //instancia do SHA1 
            using (SHA1Managed sha1 = new SHA1Managed())
            {

                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    //x minusculo para usar o minusculo
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private static DataJson DeserializeJson(string retorno)
        {
            var dataJson = JsonConvert.DeserializeObject<DataJson>(retorno);
            return dataJson;
        }

        private static string SerializeJson(DataJson dataJson)
        {
            var json = JsonConvert.SerializeObject(dataJson);
            return json;
        }


        /// <summary>
        /// Método retorna conteudo da requisicao da API
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static async Task<string> GetJsonEncript(string path)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(path))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
