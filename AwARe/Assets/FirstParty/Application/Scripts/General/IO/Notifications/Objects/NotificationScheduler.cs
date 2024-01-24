using System;
using UnityEngine;
using AwARe.Notifications.Logic;

namespace AwARe.Notifications.Objects
{
    /// <summary>
    /// enum that represents the platform the application is running on
    /// </summary>
    enum Platform
    {
        Android,
        IOS,
        Editor
    }

    /// <summary>
    /// Class <c>NotificationScheduler</c> is responsible for 
    /// </summary>
    public class NotificationScheduler : MonoBehaviour
    {

        Platform platform;

        //for background scheduling: possibly look into 'service' C# class thing:
        //https://stackoverflow.com/questions/34573109/how-to-make-an-android-app-to-always-run-in-background

        /// <summary>
        /// Unity method that is called immediately upon object creation
        /// initialises the platform enum variable
        /// </summary>
        void Awake()
        {
            #if UNITY_EDITOR
                platform = Platform.Editor;
            #elif UNITY_ANDROID
                platform = Platform.Android;
            #elif UNITY_IOS
                platform = Platform.IOS;
            #endif
        }

        /// <summary>
        /// Test method that can be transformed into the main method later on
        /// sends a notification.
        /// </summary>
        public void SendNotificationTest()
        {
            switch(platform)
            {
                case Platform.Android:
                    SendAndroidNotification("Test Title", "Test text", "No questionnaire", DateTime.Now);
                    break;
                case Platform.IOS:
                    SendIOSNotification("Test Title", "Test text", "No questionnaire", DateTime.Now.AddSeconds(1));
                    break;
                case Platform.Editor:
                    SendEditorNotification("Test Title", "Test text", "No questionnaire", DateTime.Now.AddSeconds(10));
                    break;
                default:
                    Debug.Log("No platform detected");
                    break;
            }
        }

        /// <summary>
        /// Sends a notifcation to the android platform. does nothing if called on other platforms.
        /// </summary>
        /// <param name="title">The title text of the notification.</param>
        /// <param name="body">The body text of the notification.</param>
        /// <param name="questionnaire">The questionnaire associated with the notification.</param>
        /// <param name="time">The time at which to send the notification.</param>
        private void SendAndroidNotification(string title, string body, string questionnaire, DateTime time)
        {
        #if UNITY_ANDROID
            Notification notification = new AndroidNotif();
            SetNotifParams(notification, title, body, questionnaire, time);
            notification.Send();
        #endif
        }

        /// <summary>
        /// Sends a notifcation to the IOS platform. does nothing if called on other platforms.
        /// </summary>
        /// <param name="title">The title text of the notification.</param>
        /// <param name="body">The body text of the notification.</param>
        /// <param name="questionnaire">The questionnaire associated with the notification.</param>
        /// <param name="time">The time at which to send the notification.</param>
        private void SendIOSNotification(string title, string body, string questionnaire, DateTime time)
        {
        #if UNITY_IOS
            Notification notification = new IOSNotif();
            SetNotifParams(notification, title, body, questionnaire, time);
            notification.Send();
        #endif
        }

        /// <summary>
        /// Sends a notifcation to the unity editor environment. does nothing if called on other platforms.
        /// </summary>
        /// <param name="title">The title text of the notification.</param>
        /// <param name="body">The body text of the notification.</param>
        /// <param name="questionnaire">The questionnaire associated with the notification.</param>
        /// <param name="time">The time at which to send the notification.</param>
        private void SendEditorNotification(string title, string body, string questionnaire, DateTime time)
        {
        #if UNITY_EDITOR
            Notification notification = new EditorNotif();
            SetNotifParams(notification, title, body, questionnaire, time);
            notification.Send();
        #endif
        }

        /// <summary>
        /// Sets the parameters of a notification.
        /// </summary>
        /// <param name="notification">The instance of the implementation of the Notification (ios, android or editor). </param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="body">The body of the notification.</param>
        /// <param name="questionnaire">The questionnaire attached to the notification.</param>
        /// <param name="time">The time at which the notification will be sent.</param>
        private void SetNotifParams(Notification notification, string title, string body, string questionnaire, DateTime time)
        {
            notification.SetFireTime(time);
            notification.SetTitle(title);
            notification.Setbody(body);
            notification.SetQuestionnaire(questionnaire);
        }
    }
}