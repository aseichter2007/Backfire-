using Android;
using Android.App;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backfire
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        Android.App.AlertDialog _dialogue;
        AudioRecorderService _recorder;
        System.String serverAddress= "http://172.17.114.33:52251/api/values"; 
        //xamarin debugs in a virtual machine, localhost cannot be used. find server ip with ipconfig and enter it above.


        protected override void OnCreate(Bundle savedInstanceState)
        {
            var path = this.FilesDir + "/data";
            var engineaudio = path + "/engineaudio.wav";
            var exists = Directory.Exists(path);
            var filepath = path + "/cardata.txt";

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            ImageView mufflerbar = FindViewById<ImageView>(Resource.Id.mufflerBar);
            mufflerbar.Visibility = ViewStates.Invisible;
            //set mufflerbar image

            ImageView submitimg = FindViewById<ImageView>(Resource.Id.submitimg);
            submitimg.Visibility = ViewStates.Invisible;
            //set image

            ImageView engineRecordImg = FindViewById<ImageView>(Resource.Id.imgRecordEngine);
            engineRecordImg.SetImageResource(Resource.Drawable.enginerecord);

            Button saveButton = FindViewById<Button>(Resource.Id.buttonSave);
            saveButton.Click += OnSaveButtonClicked;

            Button clearButton = FindViewById<Button>(Resource.Id.buttonClear);
            clearButton.Click += OnClearButtonClicked;

            Button buttonRecordEngine = FindViewById<Button>(Resource.Id.buttonRecordEngine);
            buttonRecordEngine.Click += OnRecordEngineClicked;

            Button buttonClearRecording = FindViewById<Button>(Resource.Id.buttonClearEngine);
            buttonClearRecording.Click += OnRecordClear;
            buttonClearRecording.Visibility = ViewStates.Gone;
            

            Button infoButton = FindViewById<Button>(Resource.Id.buttonInfo);
            infoButton.Click += OnInfoClicked;

            Button submitButton = FindViewById<Button>(Resource.Id.buttonSubmit);
            submitButton.Click += OnSubmitClicked;

            if (System.IO.File.Exists(engineaudio))
            {
                buttonRecordEngine.Visibility = ViewStates.Gone;
                buttonClearRecording.Visibility = ViewStates.Visible;
                submitButton.Visibility = ViewStates.Visible;
            }


            Button notificationbutton = FindViewById<Button>(Resource.Id.buttonNotifications);
            notificationbutton.Click += OnNotificationToggle;
            notificationbutton.Visibility = ViewStates.Invisible;//notifications not implemented.

            Button thankYoubutton = FindViewById<Button>(Resource.Id.buttonThankYou);
            thankYoubutton.Click += OnThankYou;
            if (System.IO.File.Exists(filepath))
            {
                var make = FindViewById<EditText>(Resource.Id.carMake);
                var model = FindViewById<EditText>(Resource.Id.carModel);
                var year = FindViewById<EditText>(Resource.Id.carYear);

                //get data from file so its reaty to send later.
                var carInfoFile = new StreamReader(filepath);
                var info = carInfoFile.ReadLine();
                var infoparts = info.Split('&');
                make.Text = infoparts[0];
                model.Text = infoparts[1];
                year.Text = infoparts[2];
            }
            if (System.IO.File.Exists(path + "/engineaudio.wav"))
            {
                buttonRecordEngine.Visibility = ViewStates.Gone;
                buttonClearRecording.Visibility = ViewStates.Visible;
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            var carInfoPanel = FindViewById<LinearLayout>(Resource.Id.carInfo);
            var carRecordPanel = FindViewById<LinearLayout>(Resource.Id.CarRecord);
            var carSubmission = FindViewById<LinearLayout>(Resource.Id.notifySubmit);
            var disclaimer = FindViewById<LinearLayout>(Resource.Id.disclaimer);

            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    disclaimer.Visibility = ViewStates.Gone;
                    carInfoPanel.Visibility = ViewStates.Visible;
                    carRecordPanel.Visibility = ViewStates.Gone;
                    carSubmission.Visibility = ViewStates.Gone;

                    return true;
                case Resource.Id.navigation_dashboard:
                    disclaimer.Visibility = ViewStates.Gone;
                    carInfoPanel.Visibility = ViewStates.Gone;
                    carRecordPanel.Visibility = ViewStates.Visible;
                    carSubmission.Visibility = ViewStates.Gone;
                    return true;
                case Resource.Id.navigation_notifications:
                    disclaimer.Visibility = ViewStates.Gone;
                    carInfoPanel.Visibility = ViewStates.Gone;
                    carRecordPanel.Visibility = ViewStates.Gone;
                    carSubmission.Visibility = ViewStates.Visible;
                    return true;
            }
            return false;
        }
        public async void OnSaveButtonClicked(object sender, EventArgs args)
        {
            //change to next view
            var carInfoPanel = FindViewById<LinearLayout>(Resource.Id.carInfo);
            var carRecordPanel = FindViewById<LinearLayout>(Resource.Id.CarRecord);
            carInfoPanel.Visibility = ViewStates.Gone;
            carRecordPanel.Visibility = ViewStates.Visible;

            var make = FindViewById<EditText>(Resource.Id.carMake);
            var model = FindViewById<EditText>(Resource.Id.carModel);
            var year = FindViewById<EditText>(Resource.Id.carYear);

            var path = this.FilesDir + "/data";
            var exists = Directory.Exists(path);
            var filepath = path + "/cardata.txt";
            if (!exists)
            {
                //pretty sure I threw an exception here the first time I ran this
                //and then never again. Dont know why, can't reproduce because now the directory exists. 
                //Worked when testing on device.  Not sure what data is sent to the device when debugging.
                Directory.CreateDirectory(path);
                if (!System.IO.File.Exists(filepath))
                {
                    var newfile = new Java.IO.File(path, "cardata.txt");
                    using (FileOutputStream cardata = new FileOutputStream(newfile))
                    {
                        cardata.Write(System.Text.Encoding.ASCII.GetBytes(make.Text + "&" + model.Text + "&" + year.Text));
                        cardata.Close();
                    }
                }
            }
            else
            {
                var oldcardata = new Java.IO.File(path, "cardata.txt");

                using (FileOutputStream cardata = new FileOutputStream(oldcardata))
                {
                    cardata.Write(System.Text.Encoding.ASCII.GetBytes(make.Text + "&" + model.Text + "&" + year.Text));
                    cardata.Close();
                }
            }

        }
        public async void OnClearButtonClicked(object sender, EventArgs args)
        {
            var make = FindViewById<EditText>(Resource.Id.carMake);
            var model = FindViewById<EditText>(Resource.Id.carModel);
            var year = FindViewById<EditText>(Resource.Id.carYear);
            make.Text = "";
            model.Text = "";
            year.Text = "";

        }
        public async void OnRecordEngineClicked(object sender, EventArgs args)
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != (int)Permission.Granted)
            {

                ActivityCompat.RequestPermissions(this, new System.String[] { Manifest.Permission.RecordAudio }, 1);

            }

            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Recording quality matters");
            alert.SetMessage("Please minimize background noise and wind while recording.");
            alert.SetPositiveButton("Record", OnRecordConfirm);
            alert.SetNegativeButton("cancel", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void OnDialogDismiss(object sender, EventArgs args)
        {
            if (_dialogue != null)
            {
                _dialogue.Dismiss();
                //not sure if this is actually needed but if it isnt broke...
            }

        }
        public async void OnRecordConfirm(object sender, EventArgs args)
        {
            Button buttonRecordEngine = FindViewById<Button>(Resource.Id.buttonRecordEngine);
            buttonRecordEngine.Visibility = ViewStates.Gone;

            var path = this.FilesDir + "/data";
            var engineaudio = path+ "/engineaudio.wav";

            _recorder = new AudioRecorderService();
            _recorder.StopRecordingAfterTimeout = true;
            //_recorder.StopRecordingOnSilence = true;
            _recorder.FilePath = engineaudio;

            MediaPlayer beeper = MediaPlayer.Create(this, Resource.Raw.beep);
            await WaitSeconds(3);
            beeper.Start();
            await WaitSeconds(1);
            var awaiter = await _recorder.StartRecording();//why isnt this working? 
            //I dont think its getting a valid audio source.  not sure how to debug the inner workings.

            Button buttonClearRecording = FindViewById<Button>(Resource.Id.buttonClearEngine);
            buttonClearRecording.Visibility = ViewStates.Visible;

            Button submit = FindViewById<Button>(Resource.Id.buttonSubmit);
            submit.Visibility = ViewStates.Visible;

            beeper.Start();
            await WaitSeconds(1);
            beeper.Release();
        }
        //possible alternative audio record method. not complete, needs class member variables and to be in more pieces to allow separate thread to time recording.
        //public async void OnRecordConfirmV2(object sender, EventArgs args)
        //{
        //    Button buttonRecordEngine = FindViewById<Button>(Resource.Id.buttonRecordEngine);
        //    buttonRecordEngine.Visibility = ViewStates.Gone;

        //    var path = this.FilesDir + "/data";
        //    var engineaudio = path + "/engineaudio.wav";

        //    MediaRecorder mediaRecorder = new MediaRecorder();

        //    System.IO.Stream outputStream = System.IO.File.Open(engineaudio, FileMode.Create);
        //    BinaryWriter bWriter = new BinaryWriter(outputStream);

        //    audioBuffer = new byte[8000];

        //    audRecorder = new AudioRecord(
        //        // Hardware source of recording.
        //        AudioSource.Mic,
        //        // Frequency
        //        11025,
        //        // Mono or stereo
        //        ChannelIn.Mono,
        //        // Audio encoding
        //        Android.Media.Encoding.Pcm16bit,
        //        // Length of the audio clip.
        //        audioBuffer.Length
        //    );

        //    long totalAudioLen = 0;
        //    long totalDataLen = totalAudioLen + 36;
        //    long longSampleRate = 11025;
        //    int channels = 2;
        //    long byteRate = 16 * longSampleRate * channels / 8;

        //    totalAudioLen = audioBuffer.Length;
        //    totalDataLen = totalAudioLen + 36;

        //    WriteWaveFileHeader(
        //        bWriter,
        //        totalAudioLen,
        //        totalDataLen,
        //        longSampleRate,
        //        channels,
        //        byteRate);

           
       


        ////AudioRecord audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono,Encoding.Pcm16bit, 128000);
            

        //    MediaPlayer beeper = MediaPlayer.Create(this, Resource.Raw.beep);
        //    await WaitSeconds(3);
        //    beeper.Start();
        //    await WaitSeconds(1);

        //    audRecorder.StartRecording();

        //    //while (_isRecording == true)
        //    //{
        //        try
        //        {
        //            /// Keep reading the buffer while there is audio input.
        //            audioData = audRecorder.Read(audioBuffer, 0, audioBuffer.Length);

        //            bWriter.Write(audioBuffer);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            System.Console.Out.WriteLine(ex.Message);
        //            break;
        //        }
        //   // }
           
        //    outputStream.Close();
        //    bWriter.Close();

        //    var audioid = audioRecord.AudioSessionId;
        //    var audio = FindViewById(audioid);



        //    Button buttonClearRecording = FindViewById<Button>(Resource.Id.buttonClearEngine);
        //    buttonClearRecording.Visibility = ViewStates.Visible;

        //    Button submit = FindViewById<Button>(Resource.Id.buttonSubmit);
        //    submit.Visibility = ViewStates.Visible;


        //    beeper.Start();
        //    await WaitSeconds(1);
        //    beeper.Release();
        //}
        private void WriteWaveFileHeader(BinaryWriter bWriter, long totalAudioLen, long totalDataLen, long longSampleRate, int channels, long byteRate)
        {

            byte[] header = new byte[44];

            header[0] = (byte)'R'; // RIFF/WAVE header
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            header[4] = (byte)(totalDataLen & 0xff);
            header[5] = (byte)((totalDataLen >> 8) & 0xff);
            header[6] = (byte)((totalDataLen >> 16) & 0xff);
            header[7] = (byte)((totalDataLen >> 24) & 0xff);
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            header[12] = (byte)'f'; // 'fmt ' chunk
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            header[16] = 16; // 4 bytes: size of 'fmt ' chunk
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            header[20] = 1; // format = 1
            header[21] = 0;
            header[22] = (byte)channels;
            header[23] = 0;
            header[24] = (byte)(longSampleRate & 0xff);
            header[25] = (byte)((longSampleRate >> 8) & 0xff);
            header[26] = (byte)((longSampleRate >> 16) & 0xff);
            header[27] = (byte)((longSampleRate >> 24) & 0xff);
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);
            header[32] = (byte)(2 * 16 / 8); // block align
            header[33] = 0;
            header[34] = 16; // bits per sample
            header[35] = 0;
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            header[40] = (byte)(totalAudioLen & 0xff);
            header[41] = (byte)((totalAudioLen >> 8) & 0xff);
            header[42] = (byte)((totalAudioLen >> 16) & 0xff);
            header[43] = (byte)((totalAudioLen >> 24) & 0xff);

            bWriter.Write(header, 0, 44);
        }
    
    public async Task WaitSeconds(int num)
        {
            Thread.Sleep(num*1000);
        }

        public async void OnRecordClear(object sender, EventArgs args)
        {
            var path = this.FilesDir + "/data";
            var engineaudio = path + "/engineaudio.wav";

            if (System.IO.File.Exists(engineaudio))
            {
                System.IO.File.Delete(engineaudio);
            }
            Button buttonClearRecording = FindViewById<Button>(Resource.Id.buttonClearEngine);
            buttonClearRecording.Visibility = ViewStates.Gone;

            Button buttonRecordEngine = FindViewById<Button>(Resource.Id.buttonRecordEngine);
            buttonRecordEngine.Visibility = ViewStates.Visible;

            Button submit = FindViewById<Button>(Resource.Id.buttonSubmit);
            submit.Visibility = ViewStates.Invisible;

        }
        public async void OnInfoClicked(object sender, EventArgs args)
        {
            var carInfoPanel = FindViewById<LinearLayout>(Resource.Id.carInfo);
            var carRecordPanel = FindViewById<LinearLayout>(Resource.Id.CarRecord);
            var carSubmission = FindViewById<LinearLayout>(Resource.Id.notifySubmit);
            var disclaimer = FindViewById<LinearLayout>(Resource.Id.disclaimer);

            disclaimer.Visibility = ViewStates.Visible;
            carInfoPanel.Visibility = ViewStates.Gone;
            carRecordPanel.Visibility = ViewStates.Gone;
            carSubmission.Visibility = ViewStates.Gone;
        }
        public async void OnSubmitClicked(object sender, EventArgs args)
        {
            

            var make = FindViewById<EditText>(Resource.Id.carMake);
            var model = FindViewById<EditText>(Resource.Id.carModel);
            var year = FindViewById<EditText>(Resource.Id.carYear);
            var fix = FindViewById<EditText>(Resource.Id.carFix);

            if (fix.Text.Length<1||year.Text.Length<1||model.Text.Length<1||make.Text.Length<1)
            {
                PleaseFillAllFields();                
            }
            else
            {
                Sending();
                var path = this.FilesDir + "/data";
                var engineaudio = path + "/engineaudio.wav";
                if (System.IO.File.Exists(engineaudio))
                {
                    Button submit = FindViewById<Button>(Resource.Id.buttonSubmit);
                    submit.Visibility = ViewStates.Invisible;

                    var file = System.IO.File.ReadAllBytes(engineaudio);
                    string filestring = System.Text.Encoding.UTF8.GetString(file);

                    /*var sendfile = new Dictionary<string, string>() { 
                        { "file", filestring },
                        { "make",make.Text },
                        { "model",model.Text },
                        { "year",year.Text },
                        { "fix",fix.Text } 
                    };*/

                    var contentProtoJson = new
                    {
                        file = file,
                        make = make.Text,
                        model = model.Text,
                        year = year.Text,
                        fix = fix.Text
                    };

                    var payload = JsonConvert.SerializeObject(contentProtoJson);
                    var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    HttpResponseMessage responseMessage;
                    try
                    {
                        responseMessage = await client.PostAsync(
                                              serverAddress,
                                              content
                                              );

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            ThankYouForYourContribution();
                        }
                        else
                        {
                            SomethingWentWrong();
                            //this code was hit exactly once, and after no changes made, request times out.
                            submit.Visibility = ViewStates.Visible;
                        }
                    }
                    catch (System.Exception e)
                    {
                        
                        SomethingWentWrong(e.Message);
                        submit.Visibility = ViewStates.Visible;
                    }
                }
                else
                {
                    AudioNotFound();
                }
            }
        }
        public async void Sending()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Sending data");
            alert.SetMessage("This msy take a moment, please be patient and wait for confirmation.");
            alert.SetNegativeButton("OK", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void PleaseFillAllFields()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Info missing");
            alert.SetMessage("please ensure that the make, model, year, and fix for your vehicle have been entered, then try submitting again.");
            alert.SetNegativeButton("OK", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void AudioNotFound()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Audio not found");
            alert.SetMessage("You do not have a saved recording. If your vehicle has already been repaired, enter baseline on the repair line after recording new audio.");
            alert.SetNegativeButton("OK", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void ThankYouForYourContribution()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Thank you for your contribution.");
            alert.SetMessage("With your help, we are making futuristic tools to help everyone!");
            alert.SetNegativeButton("Ok", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void SomethingWentWrong()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Something went wrong");
            alert.SetMessage("Please try submitting again. are you connected to the internet?");
            alert.SetNegativeButton("Ok", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void SomethingWentWrong(string message)
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Something went wrong");
            alert.SetMessage("Please try submitting again. are you connected to the internet? error: "+message );
            alert.SetNegativeButton("Ok", OnDialogDismiss);

            _dialogue = alert.Show();
        }
        public async void OnNotificationToggle(object sender, EventArgs args)
        {
            //todo notifications, not part of minimum reqirements.
        }
        public async void OnThankYou(object sender, EventArgs args)
        {
            var carInfoPanel = FindViewById<LinearLayout>(Resource.Id.carInfo);
            var carRecordPanel = FindViewById<LinearLayout>(Resource.Id.CarRecord);
            var carSubmission = FindViewById<LinearLayout>(Resource.Id.notifySubmit);
            var disclaimer = FindViewById<LinearLayout>(Resource.Id.disclaimer);

            disclaimer.Visibility = ViewStates.Gone;
            carInfoPanel.Visibility = ViewStates.Visible;
            carRecordPanel.Visibility = ViewStates.Gone;
            carSubmission.Visibility = ViewStates.Gone;
        }
    }
}
