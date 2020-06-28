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
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backfire
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        TextView textMessage;
        Android.App.AlertDialog _dialogue;
        AudioRecorderService _recorder;
        System.String serverAddress= "https://localhost:44322/api/values"; 


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            textMessage = FindViewById<TextView>(Resource.Id.message);
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            ImageView mufflerbar = FindViewById<ImageView>(Resource.Id.mufflerBar);
            //set mufflerbar image

            ImageView submitimg = FindViewById<ImageView>(Resource.Id.submitimg);
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

            Button infoButton = FindViewById<Button>(Resource.Id.buttonInfo);
            infoButton.Click += OnInfoClicked;

            Button submitButton = FindViewById<Button>(Resource.Id.buttonSubmit);
            submitButton.Click += OnSubmitClicked;

            Button notificationbutton = FindViewById<Button>(Resource.Id.buttonNotifications);
            notificationbutton.Click += OnNotificationToggle;
            notificationbutton.Visibility = ViewStates.Invisible;//notifications not implemented.

            Button thankYoubutton = FindViewById<Button>(Resource.Id.buttonThankYou);
            thankYoubutton.Click += OnThankYou;
            var path = this.FilesDir + "/data";
            var exists = Directory.Exists(path);
            var filepath = path + "/cardata.txt";
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
                    // textMessage.SetText(Resource.String.title_home);
                    disclaimer.Visibility = ViewStates.Gone;
                    carInfoPanel.Visibility = ViewStates.Visible;
                    carRecordPanel.Visibility = ViewStates.Gone;
                    carSubmission.Visibility = ViewStates.Gone;

                    return true;
                case Resource.Id.navigation_dashboard:
                    // textMessage.SetText(Resource.String.title_dashboard);
                    disclaimer.Visibility = ViewStates.Gone;
                    carInfoPanel.Visibility = ViewStates.Gone;
                    carRecordPanel.Visibility = ViewStates.Visible;
                    carSubmission.Visibility = ViewStates.Gone;
                    return true;
                case Resource.Id.navigation_notifications:
                    // textMessage.SetText(Resource.String.title_notifications);
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
                var oldcardata = new Java.IO.File(path, "cardata.txt");//Does this delete and create new? Seems like it does.

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
                _dialogue.Dismiss();//not sure if this is actually needed but if it isnt broke...
            }

        }
        public async void OnRecordConfirm(object sender, EventArgs args)
        {
            var path = this.FilesDir + "/data";
            var engineaudio = path+ "/engineaudio.wav";

            _recorder = new AudioRecorderService();
            _recorder.StopRecordingAfterTimeout = true;
            _recorder.StopRecordingOnSilence = true;
            _recorder.FilePath = engineaudio;

            MediaPlayer beeper = MediaPlayer.Create(this, Resource.Raw.beep);
            await WaitSeconds(3);
            beeper.Start();
            await WaitSeconds(1);
            await _recorder.StartRecording();


            Button buttonClearRecording = FindViewById<Button>(Resource.Id.buttonClearEngine);
            buttonClearRecording.Visibility = ViewStates.Visible;

            Button buttonRecordEngine = FindViewById<Button>(Resource.Id.buttonRecordEngine);
            buttonRecordEngine.Visibility = ViewStates.Gone;

            beeper.Start();
            await WaitSeconds(1);
            beeper.Release();
        }
        public async Task WaitSeconds(int num)//new thread to prevent possible interference recording
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

            var path = this.FilesDir + "/data";
            var engineaudio = path + "/engineaudio.wav";
            if (System.IO.File.Exists(engineaudio))
            {
                var file = System.IO.File.ReadAllBytes(engineaudio);
                var filestring = file.ToString(); 
                var sendfile = new Dictionary<string, string>() { 
                    { "file", filestring },
                    { "make",make.Text },
                    { "model",model.Text },
                    { "year",year.Text },
                    { "fix",fix.Text } 
                };

                var content = new FormUrlEncodedContent(sendfile);
                HttpClient client = new HttpClient();
                
                HttpResponseMessage responseMessage = await client.PostAsync(
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
                }
            }
            else
            {
                AudioNotFound();
            }
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
