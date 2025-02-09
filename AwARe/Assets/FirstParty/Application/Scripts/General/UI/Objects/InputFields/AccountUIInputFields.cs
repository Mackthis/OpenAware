// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System.Text.RegularExpressions;
using AwARe.Server.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AwARe
{
    /// <summary>
    /// Manages the UI input fields, warnings, and buttons for the account-related functionalities.
    /// </summary>
    public class AccountUIInputFields : MonoBehaviour
    {
        // input fields
        [SerializeField] private TMP_InputField loginEmailInputField;
        [SerializeField] private TMP_InputField loginPasswordInputField;
        [SerializeField] private TMP_InputField registerEmailInputField;
        [SerializeField] private TMP_InputField registerPasswordInputField;
        [SerializeField] private TMP_InputField FirstNameInputField;
        [SerializeField] private TMP_InputField LastNameInputField;
        [SerializeField] private TMP_InputField passwordConfirmInputField;

        // buttons
        [SerializeField] private Button securityButtonRegister;
        [SerializeField] private Button securityButtonLogin;
        [SerializeField] private Button referRegisterButton;
        [SerializeField] private Button referLoginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button loginButton;
        [SerializeField] private GameObject registerScreen;
        [SerializeField] private GameObject loginScreen;

        // warnings
        [SerializeField] private GameObject warningAllFields;
        [SerializeField] private GameObject warningPWEIncorrect;
        [SerializeField] private GameObject warningIncorrectEmail;
        [SerializeField] private GameObject warningWeakPW;
        [SerializeField] private GameObject warningDissimilarPW;

        // images 
        [SerializeField] private Sprite seen;
        [SerializeField] private Sprite hidden;
        private bool visibilityregsec;
        private bool visibilitylogsec;


        // Start is called before the first frame update
        void Start()
        {
            // set all warningfields to false
            warningAllFields.SetActive(false);
            warningPWEIncorrect.SetActive(false);
            warningIncorrectEmail.SetActive(false);
            warningWeakPW.SetActive(false);
            warningDissimilarPW.SetActive(false);
            registerScreen.SetActive(false);
            loginScreen.SetActive(true);
            // security toggle button is in 'hidden' mode
            visibilityregsec = false;
            visibilitylogsec = false;

            // character limit for email input fields (emails can be much longer)
            registerEmailInputField.characterLimit = 80;
            loginEmailInputField.characterLimit = 80;

            // character limit for input fields
            SetCharacterLimit(registerPasswordInputField);
            SetCharacterLimit(FirstNameInputField);
            SetCharacterLimit(LastNameInputField);
            SetCharacterLimit(passwordConfirmInputField);
            SetCharacterLimit(loginPasswordInputField);
        }


        void Awake()
        {
            securityButtonRegister.onClick.AddListener(delegate () { this.OnRSecurityButtonClick(); });
            securityButtonLogin.onClick.AddListener(delegate () { this.OnLSecurityButtonClick(); });
            referRegisterButton.onClick.AddListener(delegate () { OnReferRegisterButtonClick(); });
            referLoginButton.onClick.AddListener(delegate () { this.OnReferLoginButtonClick(); });
            registerButton.onClick.AddListener(delegate () { this.OnRegisterButtonClick(); });
            loginButton.onClick.AddListener(delegate () { this.OnLoginButtonClick(); });
        }
        /// <summary>
        /// Handles the logic when the login button is clicked.
        /// </summary>
        public async void OnLoginButtonClick()
        {
            // TODO for checking if password and email correspond with credentials saved on the server

            await Client.GetInstance().Login(new User
            {
                email = this.loginEmailInputField.text,
                password = this.loginPasswordInputField.text,
            });

            if (!await Client.GetInstance().CheckLogin())
            {
                Debug.LogError("Failed to login");
            }
            else
            {
                // Refresh the login session every 5 minutes. This is to prevent the server from logging out automatically after 15 minutes.
                GameObject.Find("ClientSetup").GetComponent<ClientSetup>().InvokeRefreshLoginSession(5 * 60);
            }
        }

        /// <summary>
        /// Switches to the register screen.
        /// </summary>
        public void OnReferRegisterButtonClick()
        {
            registerScreen.SetActive(true);
            loginScreen.SetActive(false);
        }

        /// <summary>
        /// Sets character limit for a TMP_InputField.
        /// </summary>
        /// <param name="inputField">The TMP_InputField to set the character limit for.</param>
        private void SetCharacterLimit(TMP_InputField inputField)
        {
            if (inputField != null)
            {
                // Set the character limit to 30
                inputField.characterLimit = 30;
            }
        }

        /// <summary>
        /// Switches to the login screen.
        /// </summary>
        public void OnReferLoginButtonClick()
        {
            registerScreen.SetActive(false);
            loginScreen.SetActive(true);
        }
        /// <summary>
        /// Handles the logic when the security button for registration is clicked.
        /// The register security button changes image and the password becomes visible
        /// </summary>
        public void OnRSecurityButtonClick()
        {
            Image securityimage1 = securityButtonRegister.transform.GetChild(0).GetComponent<Image>();


            if (visibilityregsec == false)
            {
                registerPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
                securityimage1.sprite = seen;
                visibilityregsec = true;

            }
            else
            {
                registerPasswordInputField.contentType = TMP_InputField.ContentType.Password;
                securityimage1.sprite = hidden;
                visibilityregsec = false;
            }

        }



        /// <summary>
        /// Handles the logic when the security button for login is clicked.
        /// The login security button changes image and the password becomes visible
        /// </summary>
        public void OnLSecurityButtonClick()
        {
            Image securityimage2 = securityButtonLogin.transform.GetChild(0).GetComponent<Image>();

            if (visibilitylogsec == false)
            {
                loginPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
                securityimage2.sprite = seen;
                visibilitylogsec = true;

            }
            else
            {
                loginPasswordInputField.contentType = TMP_InputField.ContentType.Password;
                securityimage2.sprite = hidden;
                visibilitylogsec = false;
            }

        }

        /// <summary>
        /// Handles the logic when the register button is clicked.
        /// </summary>
        public async void OnRegisterButtonClick()
        {
            warningAllFields.SetActive(false);
            //TODO : warningPWEIncorrect.SetActive(false); and something with it
            warningIncorrectEmail.SetActive(false);
            warningWeakPW.SetActive(false);
            warningDissimilarPW.SetActive(false);

            // email and password regular expressions 
            Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\-]).{12,}$");
            Regex emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");


            string registeremail = registerEmailInputField.text;
            string registerpassword = registerPasswordInputField.text;
            string registerfirstname = FirstNameInputField.text;
            string registerlastname = LastNameInputField.text;
            string registerconfirmpassword = passwordConfirmInputField.text;

            // checks if all fields are filled in 
            if (string.IsNullOrEmpty(registeremail) || string.IsNullOrEmpty(registerpassword) ||
                string.IsNullOrEmpty(registerfirstname) || string.IsNullOrEmpty(registerlastname) ||
                string.IsNullOrEmpty(registerconfirmpassword))
            {
                warningAllFields.SetActive(true);

                return; // Stop execution if any field is empty
            }

            // checks if password and confirm password are the same 
            if (registerpassword != registerconfirmpassword)
            {
                warningDissimilarPW.SetActive(true);
                return;
            }

            // checks if password is atleast 12 characters and 
            // A combination of uppercase letters, lowercase letters, numbers, and symbols.
            if (!passwordRegex.IsMatch(registerpassword) || registerpassword.Length < 12)
            {
                warningWeakPW.SetActive(true);
                return;

            }

            // checks if the email address is in an 'email-format'
            if (!emailRegex.IsMatch(registeremail))
            {
                warningIncorrectEmail.SetActive(true);
                return;
            }

            await Client.GetInstance().Register(new AccountDetails
            {
                firstName = registerfirstname,
                lastName = registerlastname,
                email = registeremail,
                password = registerpassword,
                confirmPassword = registerconfirmpassword,
            });
        }


    }
}
