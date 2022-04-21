using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SerialTestTool
{
    internal class SerialCommunication
    {
        // シングルトンのアクセス用変数
        private static SerialCommunication instance = new SerialCommunication();
        // Form用の変数
        private Form1 form;
        // SerialPort用の変数
        public SerialPort serialPort;
        // 受信リングバッファ
        private CircularBuffer<String> buffer = new CircularBuffer<string>(1048576);
        // タイムアウトの設定(https://qiita.com/satorimon/items/8993a3b088ba602dfdda)
        private CancellationTokenSource source;
        // 受信データ
        private string receivedData;
        // ストップウォッチ
        private Stopwatch stopWatch = new Stopwatch();

        /// <summary>
        /// インスタンスアクセス用メソッド
        /// </summary>
        public static SerialCommunication GetInstance
        {
            get { return instance; }
        }

        public void setForm(Form1 form)
        {
            this.form = form;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private SerialCommunication()
        {
            this.serialPort = new SerialPort();
        }

        /// <summary>
        /// シリアルポート接続
        /// </summary>
        /// <param name="portName"></param>
        /// <returns>成功: true, 失敗: false</returns>
        public bool connect(string portName)
        {
            this.disconnect(); // 既に開いている場合は閉じる
            //this.serialPort.BaudRate = 9600;
            this.serialPort.BaudRate = 3686400;
            this.serialPort.Parity = Parity.None;
            this.serialPort.StopBits = StopBits.One;
            this.serialPort.DataBits = 8;
            this.serialPort.Handshake = Handshake.None;
            this.serialPort.PortName = portName;
            this.serialPort.NewLine = "\r";
            this.serialPort.DataReceived += serialPort_DataReceived;
            try
            {
                this.serialPort.Open();
            }
            catch
            {
                return false; // エラーの場合はfalse
            }
            return true;
        }

        /// <summary>
        /// シリアルポート切断
        /// </summary>
        /// <returns></returns>
        public bool disconnect()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            return true;
        }

        private bool checkData(int nBytes, int startNum, int endNum)
        {
            for (int i = startNum; i < endNum; i++)
            {
                String data = buffer.get();
                if (!(i.ToString().PadLeft(nBytes, '0').Equals(data))) // データのチェック
                {
                    buffer.show(i + 2);
                    Debug.WriteLine($"[NG] actualy={i.ToString().PadLeft(nBytes, '0')} != data={data}");
                    return false;
                } else
                {
                    //Debug.WriteLine($"[OK] {i.ToString().PadLeft(nBytes, '0')} == data={data}");
                }
            }
            return true;
        }

        public async Task<bool> sendTestData(int nBytes, int startNum, int endNum, double interval)
        {
            bool result = false;

            if (!serialPort.IsOpen)
            {
                this.form.statusError = "COMポートに接続されていません";
                return false;
            }
            try
            {
                this.source = new CancellationTokenSource();
                this.source.CancelAfter(300000); // タイムアウト30秒
                buffer.reset();
                receivedData = "";
                stopWatch.Start();
                serialPort.WriteLine("TEST");
                for (int i = startNum; i < endNum; i++)
                {
                    serialPort.WriteLine(i.ToString().PadLeft(nBytes, '0'));
                    //Thread.Sleep(0);
                }
                serialPort.WriteLine("OK");
                if (await waitResponse("OK"))
                {
                    stopWatch.Stop(); // OKが返ってくるまでの時間
                    result = checkData(nBytes, startNum, endNum);
                } else
                {
                    this.form.statusError = "レスポンスエラーです";
                    result = false;
                }
            } catch
            {
                this.form.statusError = "レスポンスエラーです";
                return false;
            } finally
            {
                this.source.Dispose();
            }
            long elapsedMsec = stopWatch.ElapsedMilliseconds;
            this.form.setTextElapsed = $"{elapsedMsec}";
            this.form.setTextDataMsec = $"{(double)elapsedMsec / (endNum - startNum)}";
            stopWatch.Reset();
            return result;
        }

        /// <summary>
        /// レスポンス待機
        /// NOTE: レスポンスに成功したらバッファはクリア
        /// </summary>
        /// <param name="res">レスポンス文字列</param>
        /// <returns></returns>
        async public Task<bool> waitResponse(String res)
        {
            bool result = false;
            try
            {
                result = await Task.Run(() => // 非同期実行
                {
                    try
                    {
                        while (true)
                        {
                            source.Token.ThrowIfCancellationRequested();
                            if (receivedData.Contains(res)) // 受信データ内にレスポンス文字列が含まれるか
                            {
                                //Debug.WriteLine($"received=\n{receivedData}");
                                foreach (String data in receivedData.Split("\r")) // 改行ごとにリングバッファに挿入
                                {
                                    if (!(data.Equals("TEST") || data.Equals("OK")))
                                    {
                                        buffer.put(data);
                                    }
                                }
                                return true;
                            }
                            Thread.Sleep(0);
                        }
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
            }
            return result;
        }

        public void stopTest()
        {
            Debug.WriteLine(this.source.IsCancellationRequested);
        }

        /// <summary>
        /// データ受信イベント
        /// <CR>まで読み込み、バッファに格納
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (this.serialPort.BytesToRead > 0)
                {
                    String data = this.serialPort.ReadExisting(); // ReadLineだと遅い
                    receivedData += data; // 受信データを結合
                    //Debug.WriteLine(data); // DEBUG: dataを表示
                    //Debug.WriteLine("=================================");
                }
            }
            catch
            {

            }
        }
    }
}
