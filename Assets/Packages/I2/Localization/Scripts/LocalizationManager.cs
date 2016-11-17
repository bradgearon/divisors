using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace I2.Loc
{
	public static class LocalizationManager
	{
		#region Variables: CurrentLanguage
		
		public static string CurrentLanguage
		{
			get {
				InitializeIfNeeded();
				return mCurrentLanguage;
			}
			set {
				string SupportedLanguage = GetSupportedLanguage(value);
				if (!string.IsNullOrEmpty(SupportedLanguage) && mCurrentLanguage != SupportedLanguage)
				{
					SetLanguageAndCode( SupportedLanguage, GetLanguageCode(SupportedLanguage));
				}
			}
		}
		public static string CurrentLanguageCode
		{
			get { 
				InitializeIfNeeded();
				return mLanguageCode; }
			set {
				if (mLanguageCode!=value)
				{
					string LanName = GetLanguageFromCode( value );
					if (!string.IsNullOrEmpty(LanName))
						SetLanguageAndCode( LanName, value );
				}
			}
		}

		// "English (United States)" (get returns "United States") 
		// when set "Canada", the new language code will be "English (Canada)"
		public static string CurrentRegion
		{
			get { 
				var Lan = CurrentLanguage;
				int idx = Lan.IndexOfAny ("/\\".ToCharArray ());
				if (idx>0)
					return Lan.Substring(idx+1);

				idx = Lan.IndexOfAny ("[(".ToCharArray ());
				int idx2 = Lan.LastIndexOfAny ("])".ToCharArray ());
				if (idx>0 && idx!=idx2)
					return Lan.Substring(idx+1, idx2-idx-1);
				else
					return string.Empty;
			}
			set {
				var Lan = CurrentLanguage;
				int idx = Lan.IndexOfAny ("/\\".ToCharArray ());
				if (idx>0)
				{
					CurrentLanguage = Lan.Substring(idx+1) + value;
					return;
				}
				
				idx = Lan.IndexOfAny ("[(".ToCharArray ());
				int idx2 = Lan.LastIndexOfAny ("])".ToCharArray ());
				if (idx>0 && idx!=idx2)
					Lan = Lan.Substring(idx);

				CurrentLanguage = Lan + "(" + value +")";
			}
		}

		// "en-US" (get returns "US") (when set "CA", the new language code will be "en-CA")
		public static string CurrentRegionCode
		{
			get { 
					var code = CurrentLanguageCode;
					int idx = code.IndexOfAny (" -_/\\".ToCharArray ());
					return idx < 0 ? string.Empty : code.Substring (idx + 1);
				}
			set {
				var code = CurrentLanguageCode;
				int idx = code.IndexOfAny (" -_/\\".ToCharArray ());
				if (idx > 0)
					code = code.Substring (0, idx);

				CurrentLanguageCode = code + "-" + value;
			}
		}

		static string mCurrentLanguage;
		static string mLanguageCode;
		public static bool IsRight2Left = false;

		static void InitializeIfNeeded()
		{
			if (string.IsNullOrEmpty(mCurrentLanguage))
			{
				UpdateSources();
				SelectStartupLanguage();
			}
		}

		public static void SetLanguageAndCode( string LanguageName, string LanguageCode, bool RememberLanguage = true, bool Force = false )
		{
			if (mCurrentLanguage != LanguageName || mLanguageCode != LanguageCode || Force)
			{
				if (RememberLanguage)
					PlayerPrefs.SetString ("I2 Language", LanguageName);
				mCurrentLanguage = LanguageName;
				mLanguageCode = LanguageCode;
                #if NETFX_CORE
                    IsRight2Left = IsRTL (mLanguageCode);
                #else
                    System.Threading.Thread.CurrentThread.CurrentCulture = GetCulture(LanguageCode);
                    IsRight2Left = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;  //IsRTL (mLanguageCode);
                #endif
                LocalizeAll(Force);
			}
		}

        static CultureInfo GetCulture( string code )
        {
            #if !NETFX_CORE
                try
                {
                    return System.Globalization.CultureInfo.CreateSpecificCulture(code);
                }
                catch(System.Exception)
                {
                    return CultureInfo.InvariantCulture;
                }
            #else
                return CultureInfo.InvariantCulture;
            #endif
        }


        static void SelectStartupLanguage()
		{
			// Use the system language if there is a source with that language, 
			// or pick any of the languages provided by the sources

			string SavedLanguage = PlayerPrefs.GetString ("I2 Language", string.Empty);
			string SysLanguage = Application.systemLanguage.ToString();
			if (SysLanguage=="ChineseSimplified") SysLanguage = "Chinese (Simplified)";
			if (SysLanguage=="ChineseTraditional") SysLanguage = "Chinese (Traditional)";

			// Try selecting the System Language
			// But fallback to the first language found  if the System Language is not available in any source

			if (HasLanguage (SavedLanguage, Initialize:false))
			{
				CurrentLanguage = SavedLanguage;
				return;
			}

			// Check if the device language is supported. 
			// Also recognize when not region is set ("English (United State") will be used if sysLanguage is "English")
			string ValidLanguage = GetSupportedLanguage(SysLanguage);
			if (!string.IsNullOrEmpty(ValidLanguage))
			{
				SetLanguageAndCode( ValidLanguage, GetLanguageCode(ValidLanguage), false);
				return;
			}

			//--[ Use first language ]-----------
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].mLanguages.Count>0)
				{
					SetLanguageAndCode( Sources[i].mLanguages[0].Name, Sources[i].mLanguages[0].Code, false);
					return;
				}
		}

#endregion

#region Variables: Misc

		//public static Dictionary<string, string> Terms = new Dictionary<string, string>();
		public static List<LanguageSource> Sources = new List<LanguageSource>();
		public static string[] GlobalSources = new string[]{"I2Languages"};

		public delegate void OnLocalizeCallback ();
		public static event OnLocalizeCallback OnLocalizeEvent;

        public static List<ILocalizationParamsManager> ParamManagers = new List<ILocalizationParamsManager>();
#endregion

#region Localization

        public static string GetTermTranslation (string Term) { return GetTermTranslation (Term, LocalizationManager.IsRight2Left, 0, false); }
		public static string GetTermTranslation (string Term, bool FixForRTL) { return GetTermTranslation (Term, FixForRTL, 0, false); }
        public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL) { return GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, false); }
        public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers)
		{
			string Translation;
			if (TryGetTermTranslation(Term, out Translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers))
				return Translation;

			return string.Empty;
		}

		public static bool TryGetTermTranslation(string Term, out string Translation ) { return TryGetTermTranslation (Term, out Translation, false, 0, false); }
		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL ) { return TryGetTermTranslation (Term, out Translation, FixForRTL, 0, false); }
		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers )
		{
			Translation = string.Empty;
            if (string.IsNullOrEmpty(Term))
				return false;

			InitializeIfNeeded();

			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].TryGetTermTranslation (Term, out Translation))
				{
					if (LocalizationManager.IsRight2Left && FixForRTL)
						Translation = ApplyRTLfix(Translation, maxLineLengthForRTL, ignoreRTLnumbers);
                	return true;
				}

			return false;
		}

		public static string ApplyRTLfix( string line ) { return ApplyRTLfix (line, 0, false); }
		public static string ApplyRTLfix( string line, int maxCharacters, bool ignoreNumbers )
		{
            bool fixTags = true;
            bool fixIgnoreRTL = true;

            MatchCollection regexMatches2 = null;
            if (fixIgnoreRTL || ignoreNumbers)
            {
                var regex2 = new Regex( ignoreNumbers ? @"<ignoreRTL>(?<val>.*)<\/ignoreRTL>|(?<val>\d+)" : @"<ignoreRTL>(?<val>.*)<\/ignoreRTL>");
                regexMatches2 = regex2.Matches(line);
                line = regex2.Replace(line, "¬");
            }

            MatchCollection regexMatches1 = null;
            if (fixTags)
            {
                var regex1 = new Regex(@"(?></?\w+)(?>(?:[^>'""]+|'[^']*'|""[^""]*"")*)>|\[.*?\]");
                regexMatches1 = regex1.Matches(line);
                line = regex1.Replace(line, "¶");
            }

            if (maxCharacters <= 0)
            {
                line = ArabicSupport.ArabicFixer.Fix(line);
            }
            else
            {
                // Split into lines of maximum length
                var regex = new Regex(".{0," + maxCharacters + "}(\\s+|$)", RegexOptions.Multiline);
                line = line.Replace("\r\n", "\n");
                line = regex.Replace(line, "$0\n");

                line = line.Replace("\n\n", "\n");
                //if (line.EndsWith("\n\n"))
                  //  line = line.Substring(0, line.Length - 2);

                // Apply the RTL fix for each line
                var lines = line.Split('\n');
                for (int i = 0, imax = lines.Length; i < imax; ++i)
                    lines[i] = ArabicSupport.ArabicFixer.Fix(lines[i]);
                line = string.Join("\n", lines);
            }

            // restore tags
            if (fixTags && regexMatches2 != null)
            {
                int nMatches = regexMatches2.Count;
                int idx = 0;
                for (int i = nMatches-1; i>=0; --i)
                {
                    idx = line.IndexOf('¬', idx);
                    line = line.Remove(idx, 1).Insert(idx, regexMatches2[i].Groups["val"].Value);
                }
            }

            // restore ignoreRTL tags
            if (fixTags && regexMatches1 != null)
            {
                int nMatches = regexMatches1.Count;
                int idx = 0;
                for (int i = 0; i < nMatches; ++i)
                {
                    idx = line.IndexOf('¶', idx);
                    line = line.Remove(idx, 1).Insert(idx, regexMatches1[i].Value);
                }

            }
            return line;
		}

		public static string FixRTL_IfNeeded(string text, int maxCharacters = 0, bool ignoreNumber=false)
		{
			if (LocalizationManager.IsRight2Left)
				return ApplyRTLfix(text, maxCharacters, ignoreNumber);
			else
				return text;
		}

		internal static void LocalizeAll(bool Force = false)
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll( typeof(Localize) );
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize local = Locals[i];
				//if (ObjectExistInScene (local.gameObject))
				local.OnLocalize(Force);
			}
			if (OnLocalizeEvent != null)
				OnLocalizeEvent ();
			ResourceManager.pInstance.CleanResourceCache();
		}

        internal static void ApplyLocalizationParams( ref string translation, Localize localizeCmp )
        {
            var regex = new Regex(@"{\[(.*?)\]}");
            var regexMatches = regex.Matches(translation);
            for (int i = 0, nMatches = regexMatches.Count; i < nMatches; ++i)
            {
                var match = regexMatches[i];
                var param = match.Groups[match.Groups.Count - 1].Value;
                var result = GetLocalizationParam(param, localizeCmp);
                if (result!=null)
                    translation = translation.Replace(match.Value, result);
            }
        }

        internal static string GetLocalizationParam( string ParamName, Localize localizeCmp )
        {
            string result = null;
            if (localizeCmp)
            {
                var components = localizeCmp.GetComponents<MonoBehaviour>();
                for (int i=0, imax=components.Length; i<imax; ++i)
                {
                    var manager = components[i] as ILocalizationParamsManager;
                    if (manager != null)
                    {
                        result = manager.GetParameterValue(ParamName);
                        if (result != null)
                            return result;
                    }
                }
            }

            for (int i = 0, imax = ParamManagers.Count; i < imax; ++i)
            {
                result = ParamManagers[i].GetParameterValue(ParamName);
                if (result!=null)
                    return result;
            }

            return null;
        }

#endregion

#region Sources

		public static bool UpdateSources()
		{
			UnregisterDeletededSources();
			RegisterSourceInResources();
			RegisterSceneSources();
            return Sources.Count>0;
        }

		static void UnregisterDeletededSources()
		{
			// Delete sources that were part of another scene and not longer available
			for (int i=Sources.Count-1; i>=0; --i)
				if (Sources[i] == null)
					RemoveSource( Sources[i] );
		}

		static void RegisterSceneSources()
		{
			LanguageSource[] SceneSources = (LanguageSource[])Resources.FindObjectsOfTypeAll( typeof(LanguageSource) );
			for (int i=0, imax=SceneSources.Length; i<imax; ++i)
				if (!Sources.Contains(SceneSources[i]))
				{
					AddSource( SceneSources[i] );
				}
		}		

		static void RegisterSourceInResources()
		{
			// Find the Source that its on the Resources Folder
			foreach (string SourceName in GlobalSources)
			{
				GameObject Prefab = (ResourceManager.pInstance.GetAsset<GameObject>(SourceName));
				LanguageSource GlobalSource = (Prefab ? Prefab.GetComponent<LanguageSource>() : null);
				
				if (GlobalSource && !Sources.Contains(GlobalSource))
					AddSource( GlobalSource );
			}
		}		

		internal static void AddSource ( LanguageSource Source )
		{
			if (Sources.Contains (Source))
				return;

            Sources.Add( Source );
#if !UNITY_EDITOR || I2LOC_AUTOSYNC_IN_EDITOR
			Source.Import_Google_FromCache ();
			if (Source.GoogleUpdateDelay > 0)
					Source.Invoke ("Delayed_Import_Google", Source.GoogleUpdateDelay);
			else
					Source.Import_Google();
#endif
			if (Source.mDictionary.Count==0)
				Source.UpdateDictionary(true);
		}

		internal static void RemoveSource (LanguageSource Source )
		{
			//Debug.Log ("RemoveSource " + Source+" " + Source.GetInstanceID());
			Sources.Remove( Source );
		}

		public static bool IsGlobalSource( string SourceName )
		{
			return System.Array.IndexOf(GlobalSources, SourceName)>=0;
		}

		public static bool HasLanguage( string Language, bool AllowDiscartingRegion = true, bool Initialize=true )
		{
			if (Initialize)
				LocalizationManager.InitializeIfNeeded();

			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].GetLanguageIndex(Language, false)>=0)
					return true;

			// Then allow matching "English (Canada)" to "english"
			if (AllowDiscartingRegion)
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
					if (Sources[i].GetLanguageIndex(Language, true)>=0)
						return true;
			}
			return false;
		}

		// Returns the provided language or a similar one without the Region 
		//(e.g. "English (Canada)" could be mapped to "english" or "English (United States)" if "English (Canada)" is not found
		public static string GetSupportedLanguage( string Language )
		{
			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, false);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			
			// Then allow matching "English (Canada)" to "english"
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, true);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}

			return string.Empty;
		}

		public static string GetLanguageCode( string Language )
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Code;
			}
			return string.Empty;
		}

		public static string GetLanguageFromCode( string Code )
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndexFromCode(Code);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			return string.Empty;
		}


		public static List<string> GetAllLanguages ()
		{
			List<string> Languages = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				for (int j=0, jmax=Sources[i].mLanguages.Count; j<jmax; ++j)
				{
					if (!Languages.Contains(Sources[i].mLanguages[j].Name))
						Languages.Add(Sources[i].mLanguages[j].Name);
				}
			}
			return Languages;
		}

		public static List<string> GetCategories ()
		{
			List<string> Categories = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Sources[i].GetCategories(false, Categories);
            return Categories;
        }



		public static List<string> GetTermsList ()
		{
			if (Sources.Count==0)
				UpdateSources();

			if (Sources.Count==1)
				return Sources[0].GetTermsList();

			HashSet<string> Terms = new HashSet<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Terms.UnionWith( Sources[i].GetTermsList() );
			return new List<string>(Terms);
		}

		public static TermData GetTermData( string term )
		{
			TermData data;
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				data = Sources[i].GetTermData(term);
				if (data!=null)
					return data;
			}

			return null;
		}

		public static LanguageSource GetSourceContaining( string term, bool fallbackToFirst = true )
		{
			if (!string.IsNullOrEmpty(term))
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
				{
					if (Sources[i].GetTermData(term) != null)
						return Sources[i];
	            }
			}
            
            return ((fallbackToFirst && Sources.Count>0) ? Sources[0] :  null);
		}
        

#endregion

		public static Object FindAsset (string value)
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Object Obj = Sources[i].FindAsset(value);
				if (Obj)
					return Obj;
			}
			return null;
		}

		public static string GetVersion()
		{
			return "2.6.8 f4";
		}

		public static int GetRequiredWebServiceVersion()
		{
			return 4;
		}

        public static string GetWebServiceURL( LanguageSource source = null )
        {
            if (source != null && !string.IsNullOrEmpty(source.Google_WebServiceURL))
                return source.Google_WebServiceURL;

            for (int i = 0; i < Sources.Count; ++i)
                if (Sources[i] != null && !string.IsNullOrEmpty(Sources[i].Google_WebServiceURL))
                    return Sources[i].Google_WebServiceURL;
            return string.Empty;
        }

#region Left to Right Languages

		static string[] LanguagesRTL = {"ar-DZ", "ar","ar-BH","ar-EG","ar-IQ","ar-JO","ar-KW","ar-LB","ar-LY","ar-MA","ar-OM","ar-QA","ar-SA","ar-SY","ar-TN","ar-AE","ar-YE",
										"he","ur","ji"};

		static bool IsRTL(string Code)
		{
			return System.Array.IndexOf(LanguagesRTL, Code)>=0;
		}

#endregion

#if UNITY_EDITOR
		// This function should only be called from within the Localize Inspector to temporaly preview that Language

		public static void PreviewLanguage(string NewLanguage)
		{
			mCurrentLanguage = NewLanguage;
		}
#endif
	}

	public class TermsPopup : PropertyAttribute {}
}