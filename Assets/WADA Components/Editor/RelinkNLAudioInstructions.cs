using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wada.EditorTools
{
    /// <summary>
    /// Repoints InstructionManager.AudioInstructions_NL at the Dutch instruction objects.
    ///
    /// The NL dictionary started life as a copy of the English one, so every value still refers to
    /// a PSAInstruction under Decor/Audio/Instructions/EN. For each entry this tool takes the
    /// object it currently points at, works out where that object sits relative to the EN root,
    /// and assigns the object at the very same relative path under the NL root instead.
    ///
    /// Run the dry run first: it reports exactly what it would change and touches nothing.
    /// </summary>
    public static class RelinkNLAudioInstructions
    {
        const string EnglishRoot = "Decor/Audio/Instructions/EN";
        const string DutchRoot = "Decor/Audio/Instructions/NL";


        [MenuItem( "WADA/Relink NL Audio Instructions (dry run)", false, 100 )]
        static void DryRun()
        {
            Run( true );
        }

        [MenuItem( "WADA/Relink NL Audio Instructions", false, 101 )]
        static void Relink()
        {
            Run( false );
        }


        static void Run(bool dryRun)
        {
            InstructionManager[] managers = UnityEngine.Object.FindObjectsByType<InstructionManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None );

            if ( managers.Length != 1 )
            {
                Debug.LogError( "[RelinkNLAudioInstructions] Expected exactly one InstructionManager in the open scene(s), found " +
                                managers.Length + "." );
                return;
            }

            InstructionManager manager = managers[ 0 ];

            Scene scene = manager.gameObject.scene;
            Transform englishRoot = FindByPath( scene, EnglishRoot );
            Transform dutchRoot = FindByPath( scene, DutchRoot );

            if ( englishRoot == null || dutchRoot == null )
            {
                Debug.LogError( "[RelinkNLAudioInstructions] Could not find '" +
                                ( englishRoot == null ? EnglishRoot : DutchRoot ) + "' in scene " + scene.name );
                return;
            }

            SerializedObject so = new SerializedObject( manager );
            SerializedProperty englishKeys = so.FindProperty( "AudioInstructions.m_keys" );
            SerializedProperty englishValues = so.FindProperty( "AudioInstructions.m_values" );
            SerializedProperty dutchKeys = so.FindProperty( "AudioInstructions_NL.m_keys" );
            SerializedProperty dutchValues = so.FindProperty( "AudioInstructions_NL.m_values" );

            if ( dutchKeys == null || dutchValues == null || englishKeys == null || englishValues == null )
            {
                Debug.LogError( "[RelinkNLAudioInstructions] Could not read the dictionaries. Did SerializableDictionary change?" );
                return;
            }

            StringBuilder report = new StringBuilder();
            int relinked = 0;
            int alreadyDutch = 0;
            int failed = 0;

            for ( int i = 0; i < dutchValues.arraySize; i++ )
            {
                string key = KeyName( dutchKeys, i );

                SerializedProperty valueProperty = dutchValues.GetArrayElementAtIndex( i );
                Component current = valueProperty.objectReferenceValue as Component;

                // Nothing assigned in NL? Then take the name from the English entry with the same key.
                if ( current == null )
                {
                    current = MatchingEnglishValue( englishKeys, englishValues, dutchKeys, i );
                }

                if ( current == null )
                {
                    report.AppendLine( "  " + key + ": nothing to go on (both EN and NL are empty) - skipped" );
                    failed++;
                    continue;
                }

                if ( IsUnder( current.transform, dutchRoot ) )
                {
                    alreadyDutch++;
                    continue;
                }

                string relativePath = RelativePath( current.transform, englishRoot );
                if ( relativePath == null )
                {
                    report.AppendLine( "  " + key + ": '" + current.name + "' is not under " + EnglishRoot + " - skipped" );
                    failed++;
                    continue;
                }

                Transform dutchObject = dutchRoot.Find( relativePath );
                if ( dutchObject == null )
                {
                    report.AppendLine( "  " + key + ": no '" + relativePath + "' under " + DutchRoot + " - skipped" );
                    failed++;
                    continue;
                }

                AbstractInstruction dutchInstruction = dutchObject.GetComponent<AbstractInstruction>();
                if ( dutchInstruction == null )
                {
                    report.AppendLine( "  " + key + ": '" + relativePath + "' under " + DutchRoot +
                                       " has no instruction component - skipped" );
                    failed++;
                    continue;
                }

                report.AppendLine( "  " + key + ": " + relativePath + "  EN -> NL" );
                relinked++;

                if ( !dryRun )
                {
                    valueProperty.objectReferenceValue = dutchInstruction;
                }
            }

            if ( !dryRun && relinked > 0 )
            {
                so.ApplyModifiedProperties(); // registers its own undo step
                EditorSceneManager.MarkSceneDirty( scene );
            }

            string header = "[RelinkNLAudioInstructions] " + ( dryRun ? "DRY RUN: " : "" ) +
                            relinked + ( dryRun ? " entries would be relinked" : " entries relinked" ) +
                            ", " + alreadyDutch + " already Dutch, " + failed + " skipped." +
                            ( dryRun || relinked == 0 ? "" : " Save the scene to keep it." );

            if ( failed > 0 )
            {
                Debug.LogWarning( header + "\n" + report );
            }
            else
            {
                Debug.Log( header + "\n" + report );
            }
        }


        /// <summary>The English value stored under the same key as NL entry <paramref name="dutchIndex"/>.</summary>
        static Component MatchingEnglishValue(SerializedProperty englishKeys, SerializedProperty englishValues,
                                              SerializedProperty dutchKeys, int dutchIndex)
        {
            int wanted = dutchKeys.GetArrayElementAtIndex( dutchIndex ).intValue;

            for ( int i = 0; i < englishKeys.arraySize && i < englishValues.arraySize; i++ )
            {
                if ( englishKeys.GetArrayElementAtIndex( i ).intValue == wanted )
                {
                    return englishValues.GetArrayElementAtIndex( i ).objectReferenceValue as Component;
                }
            }

            return null;
        }


        static string KeyName(SerializedProperty keys, int index)
        {
            if ( index >= keys.arraySize )
            {
                return "?";
            }

            int value = keys.GetArrayElementAtIndex( index ).intValue;
            string name = Enum.GetName( typeof( AudioInstructionType ), value );
            return name ?? ( "(" + value + ")" );
        }


        static bool IsUnder(Transform transform, Transform root)
        {
            return RelativePath( transform, root ) != null;
        }


        /// <summary>Path of <paramref name="transform"/> below <paramref name="root"/>, or null if it is not below it.</summary>
        static string RelativePath(Transform transform, Transform root)
        {
            if ( transform == root )
            {
                return "";
            }

            string path = transform.name;
            Transform parent = transform.parent;

            while ( parent != null && parent != root )
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return parent == root ? path : null;
        }


        /// <summary>Like GameObject.Find, but it also sees inactive objects.</summary>
        static Transform FindByPath(Scene scene, string path)
        {
            int split = path.IndexOf( '/' );
            string rootName = split < 0 ? path : path.Substring( 0, split );

            foreach ( GameObject root in scene.GetRootGameObjects() )
            {
                if ( root.name != rootName )
                {
                    continue;
                }

                return split < 0 ? root.transform : root.transform.Find( path.Substring( split + 1 ) );
            }

            return null;
        }
    }
}
