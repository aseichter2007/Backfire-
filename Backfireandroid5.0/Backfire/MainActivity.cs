using Android.App;
using Android.Content.PM;
using Android.Media;
using Android.Net.Http;
using Android.Net.Rtp;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backfire
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,  ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        TextView textMessage;
        Android.App.AlertDialog _dialogue ;
        MediaRecorder _recorder;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            textMessage = FindViewById<TextView>(Resource.Id.message);
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            Button saveButton = FindViewById<Button>(Resource.Id.buttonSave);
            saveButton.Click +=  OnSaveButtonClicked;

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

                //Todo: get data from file

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
                //pretty sure I threw an exception here the first time I ran this I got an exception 
                //and then never again. Dont know why, can't reproduce because now the directory exists.
                Directory.CreateDirectory(path);
                if (!System.IO.File.Exists(filepath))
                {
                    var newfile = new Java.IO.File(path, "cardata.txt");
                    using (FileOutputStream cardata = new FileOutputStream(newfile))
                    {                        
                        cardata.Write(System.Text.Encoding.ASCII.GetBytes(make.Text+"&"+model.Text+"&"+year.Text));
                        cardata.Close();
                    }
                }
            }
            else
            {
                var oldcardata = new Java.IO.File(path, "cardata.txt");//does this delete and create new?

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
            }

        }
        public async void OnRecordConfirm(object sender, EventArgs args)
        {

            var path = this.FilesDir + "/data";
            var engineaudio = new Java.IO.File(path, "engineaudio.3gp");
            MediaRecorder mediaRecorder = new MediaRecorder();
            mediaRecorder.SetAudioSource(AudioSource.Mic);
            mediaRecorder.SetAudioEncoder(AudioEncoder.AmrNb);
            mediaRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
            mediaRecorder.SetAudioEncodingBitRate(128);
            mediaRecorder.SetOutputFile(engineaudio);
            _recorder = mediaRecorder;
            try
            {
                _recorder.Prepare();
            }
            catch (Java.IO.IOException exception)
            {
                _recorder.Reset();
                _recorder.Release();
                _recorder = null;
                OnRecordConfirm(sender, args);
            }
            _recorder.Start();

            var awaiter = await Waiter();

            _recorder.Stop();
            _recorder.Release();
            _recorder = null;
        }
        public async Task<bool> Waiter()
        {
            Thread.Sleep(20000);
            return true;
        }
        public async void RecordingFinished(object sender, EventArgs args)
        {

        }
        public async void OnRecordClear(object sender, EventArgs args)
        {
            var path = this.FilesDir + "/data";
            var engineaudio = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "/data/engineaudio.3gp");


            var audiofile = new Java.IO.File(path, "engineaudio.3gp");
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
            var url = "localhost:port/api/values";

            var make = FindViewById<EditText>(Resource.Id.carMake);
            var model = FindViewById<EditText>(Resource.Id.carModel);
            var year = FindViewById<EditText>(Resource.Id.carYear);
            var fix = FindViewById<EditText>(Resource.Id.carFix);
            var formatmodel = model.Text.Replace(' ', '_');
            formatmodel = formatmodel.Replace(',', '-');
            var formatfix = fix.Text.Replace(' ', '_');
            formatfix = formatfix.Replace(',', '-');

            var path = this.FilesDir + "/data";
            var file = path + "engineaudio.3gp";

            string audio; //todo: get audio into a format for sending off. 
            var fileout = new Java.IO.FileInputStream(file);
            

            HttpClient client = new HttpClient();
            //HttpResponseMessage responseMessage= await client.PostAsync(
            //    url +"+"+ make.Text + "+" + formatmodel + "+" + formatfix,
            //    //todo: send audio in post body
            //    );

            
            //if (responseMessage.IsSuccessStatusCode)
            //{
            //    ThankYouForYourContribution();
            //}
            //else
            //{
            //    SomethingWentWrong();
            //}
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

