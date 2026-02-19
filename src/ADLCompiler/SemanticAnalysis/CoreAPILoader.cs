namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Helper class for loading core Android API definitions
/// </summary>
public static class CoreAPILoader
{
    public static void LoadCoreAPIs(SDKBundle bundle, int apiLevel)
    {
        LoadActivityAPIs(bundle, apiLevel);
        LoadViewAPIs(bundle, apiLevel);
        LoadViewGroupAPIs(bundle, apiLevel);
        LoadLayoutManagerAPIs(bundle, apiLevel);
        LoadUIComponentAPIs(bundle, apiLevel);
        LoadContextAPIs(bundle, apiLevel);
        LoadIntentAPIs(bundle, apiLevel);
        LoadStorageAPIs(bundle, apiLevel);
        LoadFileIOAPIs(bundle, apiLevel);
        LoadVersionSpecificAPIs(bundle, apiLevel);
    }

    private static void LoadActivityAPIs(SDKBundle bundle, int apiLevel)
    {
        var activityAPI = new APIDefinition("android.app.Activity", 23);
        activityAPI.Methods.Add(new MethodSignature("onCreate", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("savedInstanceState", "android.os.Bundle") }
        });
        activityAPI.Methods.Add(new MethodSignature("onStart", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onResume", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onPause", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onStop", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onDestroy", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onRestart", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("setContentView", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("layoutResID", "int") }
        });
        activityAPI.Methods.Add(new MethodSignature("findViewById", "android.view.View", 23)
        {
            Parameters = new List<ParameterInfo> { new("id", "int") }
        });
        activityAPI.Methods.Add(new MethodSignature("finish", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("getIntent", "android.content.Intent", 23));
        activityAPI.Methods.Add(new MethodSignature("startActivityForResult", "void", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("intent", "android.content.Intent"),
                new("requestCode", "int")
            }
        });
        bundle.APIs.Add(activityAPI);
    }

    private static void LoadViewAPIs(SDKBundle bundle, int apiLevel)
    {
        var viewAPI = new APIDefinition("android.view.View", 23);
        viewAPI.Methods.Add(new MethodSignature("setOnClickListener", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("listener", "android.view.View.OnClickListener") }
        });
        viewAPI.Methods.Add(new MethodSignature("setVisibility", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("visibility", "int") }
        });
        viewAPI.Methods.Add(new MethodSignature("setEnabled", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("enabled", "boolean") }
        });
        viewAPI.Methods.Add(new MethodSignature("setBackgroundColor", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("color", "int") }
        });
        viewAPI.Methods.Add(new MethodSignature("setPadding", "void", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("left", "int"),
                new("top", "int"),
                new("right", "int"),
                new("bottom", "int")
            }
        });
        viewAPI.Methods.Add(new MethodSignature("getId", "int", 23));
        viewAPI.Methods.Add(new MethodSignature("getWidth", "int", 23));
        viewAPI.Methods.Add(new MethodSignature("getHeight", "int", 23));
        viewAPI.Fields.Add(new FieldSignature("VISIBLE", "int", 23) { IsStatic = true });
        viewAPI.Fields.Add(new FieldSignature("INVISIBLE", "int", 23) { IsStatic = true });
        viewAPI.Fields.Add(new FieldSignature("GONE", "int", 23) { IsStatic = true });
        bundle.APIs.Add(viewAPI);
    }

    private static void LoadViewGroupAPIs(SDKBundle bundle, int apiLevel)
    {
        var viewGroupAPI = new APIDefinition("android.view.ViewGroup", 23);
        viewGroupAPI.Methods.Add(new MethodSignature("addView", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("child", "android.view.View") }
        });
        viewGroupAPI.Methods.Add(new MethodSignature("removeView", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("view", "android.view.View") }
        });
        viewGroupAPI.Methods.Add(new MethodSignature("removeAllViews", "void", 23));
        viewGroupAPI.Methods.Add(new MethodSignature("getChildAt", "android.view.View", 23)
        {
            Parameters = new List<ParameterInfo> { new("index", "int") }
        });
        viewGroupAPI.Methods.Add(new MethodSignature("getChildCount", "int", 23));
        bundle.APIs.Add(viewGroupAPI);
    }

    private static void LoadLayoutManagerAPIs(SDKBundle bundle, int apiLevel)
    {
        // LinearLayout
        var linearLayoutAPI = new APIDefinition("android.widget.LinearLayout", 23);
        linearLayoutAPI.Methods.Add(new MethodSignature("setOrientation", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("orientation", "int") }
        });
        linearLayoutAPI.Methods.Add(new MethodSignature("setGravity", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("gravity", "int") }
        });
        linearLayoutAPI.Fields.Add(new FieldSignature("HORIZONTAL", "int", 23) { IsStatic = true });
        linearLayoutAPI.Fields.Add(new FieldSignature("VERTICAL", "int", 23) { IsStatic = true });
        bundle.APIs.Add(linearLayoutAPI);

        // RelativeLayout
        var relativeLayoutAPI = new APIDefinition("android.widget.RelativeLayout", 23);
        relativeLayoutAPI.Methods.Add(new MethodSignature("setGravity", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("gravity", "int") }
        });
        bundle.APIs.Add(relativeLayoutAPI);

        // FrameLayout
        var frameLayoutAPI = new APIDefinition("android.widget.FrameLayout", 23);
        frameLayoutAPI.Methods.Add(new MethodSignature("setForegroundGravity", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("gravity", "int") }
        });
        bundle.APIs.Add(frameLayoutAPI);

        // ConstraintLayout (API 23+)
        var constraintLayoutAPI = new APIDefinition("androidx.constraintlayout.widget.ConstraintLayout", 23);
        bundle.APIs.Add(constraintLayoutAPI);
    }

    private static void LoadUIComponentAPIs(SDKBundle bundle, int apiLevel)
    {
        // Button
        var buttonAPI = new APIDefinition("android.widget.Button", 23);
        buttonAPI.Methods.Add(new MethodSignature("setText", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("text", "CharSequence") }
        });
        buttonAPI.Methods.Add(new MethodSignature("getText", "CharSequence", 23));
        bundle.APIs.Add(buttonAPI);

        // TextView
        var textViewAPI = new APIDefinition("android.widget.TextView", 23);
        textViewAPI.Methods.Add(new MethodSignature("setText", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("text", "CharSequence") }
        });
        textViewAPI.Methods.Add(new MethodSignature("getText", "CharSequence", 23));
        textViewAPI.Methods.Add(new MethodSignature("setTextSize", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("size", "float") }
        });
        textViewAPI.Methods.Add(new MethodSignature("setTextColor", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("color", "int") }
        });
        bundle.APIs.Add(textViewAPI);

        // EditText
        var editTextAPI = new APIDefinition("android.widget.EditText", 23);
        editTextAPI.Methods.Add(new MethodSignature("setText", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("text", "CharSequence") }
        });
        editTextAPI.Methods.Add(new MethodSignature("getText", "android.text.Editable", 23));
        editTextAPI.Methods.Add(new MethodSignature("setHint", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("hint", "CharSequence") }
        });
        editTextAPI.Methods.Add(new MethodSignature("setInputType", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("type", "int") }
        });
        bundle.APIs.Add(editTextAPI);

        // ImageView
        var imageViewAPI = new APIDefinition("android.widget.ImageView", 23);
        imageViewAPI.Methods.Add(new MethodSignature("setImageResource", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("resId", "int") }
        });
        imageViewAPI.Methods.Add(new MethodSignature("setImageBitmap", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("bm", "android.graphics.Bitmap") }
        });
        imageViewAPI.Methods.Add(new MethodSignature("setScaleType", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("scaleType", "android.widget.ImageView.ScaleType") }
        });
        bundle.APIs.Add(imageViewAPI);

        // RecyclerView
        var recyclerViewAPI = new APIDefinition("androidx.recyclerview.widget.RecyclerView", 23);
        recyclerViewAPI.Methods.Add(new MethodSignature("setAdapter", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("adapter", "androidx.recyclerview.widget.RecyclerView.Adapter") }
        });
        recyclerViewAPI.Methods.Add(new MethodSignature("setLayoutManager", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("layout", "androidx.recyclerview.widget.RecyclerView.LayoutManager") }
        });
        bundle.APIs.Add(recyclerViewAPI);

        // ListView
        var listViewAPI = new APIDefinition("android.widget.ListView", 23);
        listViewAPI.Methods.Add(new MethodSignature("setAdapter", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("adapter", "android.widget.ListAdapter") }
        });
        bundle.APIs.Add(listViewAPI);

        // ScrollView
        var scrollViewAPI = new APIDefinition("android.widget.ScrollView", 23);
        scrollViewAPI.Methods.Add(new MethodSignature("fullScroll", "boolean", 23)
        {
            Parameters = new List<ParameterInfo> { new("direction", "int") }
        });
        bundle.APIs.Add(scrollViewAPI);
    }

    private static void LoadContextAPIs(SDKBundle bundle, int apiLevel)
    {
        var contextAPI = new APIDefinition("android.content.Context", 23);
        contextAPI.Methods.Add(new MethodSignature("getSharedPreferences", "android.content.SharedPreferences", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("mode", "int")
            }
        });
        contextAPI.Methods.Add(new MethodSignature("startActivity", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("intent", "android.content.Intent") }
        });
        contextAPI.Methods.Add(new MethodSignature("getSystemService", "Object", 23)
        {
            Parameters = new List<ParameterInfo> { new("name", "String") }
        });
        contextAPI.Methods.Add(new MethodSignature("getResources", "android.content.res.Resources", 23));
        contextAPI.Methods.Add(new MethodSignature("getPackageName", "String", 23));
        contextAPI.Methods.Add(new MethodSignature("getFilesDir", "java.io.File", 23));
        contextAPI.Methods.Add(new MethodSignature("getCacheDir", "java.io.File", 23));
        contextAPI.Methods.Add(new MethodSignature("openFileInput", "java.io.FileInputStream", 23)
        {
            Parameters = new List<ParameterInfo> { new("name", "String") }
        });
        contextAPI.Methods.Add(new MethodSignature("openFileOutput", "java.io.FileOutputStream", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("mode", "int")
            }
        });
        contextAPI.Fields.Add(new FieldSignature("MODE_PRIVATE", "int", 23) { IsStatic = true });
        contextAPI.Fields.Add(new FieldSignature("MODE_APPEND", "int", 23) { IsStatic = true });
        bundle.APIs.Add(contextAPI);
    }

    private static void LoadIntentAPIs(SDKBundle bundle, int apiLevel)
    {
        var intentAPI = new APIDefinition("android.content.Intent", 23);
        intentAPI.Methods.Add(new MethodSignature("putExtra", "android.content.Intent", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("value", "String")
            }
        });
        intentAPI.Methods.Add(new MethodSignature("getStringExtra", "String", 23)
        {
            Parameters = new List<ParameterInfo> { new("name", "String") }
        });
        intentAPI.Methods.Add(new MethodSignature("getIntExtra", "int", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("defaultValue", "int")
            }
        });
        intentAPI.Methods.Add(new MethodSignature("getBooleanExtra", "boolean", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("defaultValue", "boolean")
            }
        });
        intentAPI.Methods.Add(new MethodSignature("setAction", "android.content.Intent", 23)
        {
            Parameters = new List<ParameterInfo> { new("action", "String") }
        });
        intentAPI.Methods.Add(new MethodSignature("getAction", "String", 23));
        intentAPI.Fields.Add(new FieldSignature("ACTION_VIEW", "String", 23) { IsStatic = true });
        intentAPI.Fields.Add(new FieldSignature("ACTION_SEND", "String", 23) { IsStatic = true });
        bundle.APIs.Add(intentAPI);
    }

    private static void LoadStorageAPIs(SDKBundle bundle, int apiLevel)
    {
        // SharedPreferences
        var sharedPrefsAPI = new APIDefinition("android.content.SharedPreferences", 23);
        sharedPrefsAPI.Methods.Add(new MethodSignature("getString", "String", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("defValue", "String")
            }
        });
        sharedPrefsAPI.Methods.Add(new MethodSignature("getInt", "int", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("defValue", "int")
            }
        });
        sharedPrefsAPI.Methods.Add(new MethodSignature("getBoolean", "boolean", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("defValue", "boolean")
            }
        });
        sharedPrefsAPI.Methods.Add(new MethodSignature("getFloat", "float", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("defValue", "float")
            }
        });
        sharedPrefsAPI.Methods.Add(new MethodSignature("getLong", "long", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("defValue", "long")
            }
        });
        sharedPrefsAPI.Methods.Add(new MethodSignature("edit", "android.content.SharedPreferences.Editor", 23));
        bundle.APIs.Add(sharedPrefsAPI);

        // SharedPreferences.Editor
        var editorAPI = new APIDefinition("android.content.SharedPreferences.Editor", 23);
        editorAPI.Methods.Add(new MethodSignature("putString", "android.content.SharedPreferences.Editor", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("value", "String")
            }
        });
        editorAPI.Methods.Add(new MethodSignature("putInt", "android.content.SharedPreferences.Editor", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("value", "int")
            }
        });
        editorAPI.Methods.Add(new MethodSignature("putBoolean", "android.content.SharedPreferences.Editor", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("value", "boolean")
            }
        });
        editorAPI.Methods.Add(new MethodSignature("putFloat", "android.content.SharedPreferences.Editor", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("value", "float")
            }
        });
        editorAPI.Methods.Add(new MethodSignature("putLong", "android.content.SharedPreferences.Editor", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("key", "String"),
                new("value", "long")
            }
        });
        editorAPI.Methods.Add(new MethodSignature("remove", "android.content.SharedPreferences.Editor", 23)
        {
            Parameters = new List<ParameterInfo> { new("key", "String") }
        });
        editorAPI.Methods.Add(new MethodSignature("clear", "android.content.SharedPreferences.Editor", 23));
        editorAPI.Methods.Add(new MethodSignature("commit", "boolean", 23));
        editorAPI.Methods.Add(new MethodSignature("apply", "void", 23));
        bundle.APIs.Add(editorAPI);

        // SQLiteDatabase
        var sqliteAPI = new APIDefinition("android.database.sqlite.SQLiteDatabase", 23);
        sqliteAPI.Methods.Add(new MethodSignature("execSQL", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("sql", "String") }
        });
        sqliteAPI.Methods.Add(new MethodSignature("rawQuery", "android.database.Cursor", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("sql", "String"),
                new("selectionArgs", "String[]")
            }
        });
        sqliteAPI.Methods.Add(new MethodSignature("insert", "long", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("table", "String"),
                new("nullColumnHack", "String"),
                new("values", "android.content.ContentValues")
            }
        });
        sqliteAPI.Methods.Add(new MethodSignature("update", "int", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("table", "String"),
                new("values", "android.content.ContentValues"),
                new("whereClause", "String"),
                new("whereArgs", "String[]")
            }
        });
        sqliteAPI.Methods.Add(new MethodSignature("delete", "int", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("table", "String"),
                new("whereClause", "String"),
                new("whereArgs", "String[]")
            }
        });
        sqliteAPI.Methods.Add(new MethodSignature("close", "void", 23));
        bundle.APIs.Add(sqliteAPI);

        // SQLiteOpenHelper
        var sqliteHelperAPI = new APIDefinition("android.database.sqlite.SQLiteOpenHelper", 23);
        sqliteHelperAPI.Methods.Add(new MethodSignature("getWritableDatabase", "android.database.sqlite.SQLiteDatabase", 23));
        sqliteHelperAPI.Methods.Add(new MethodSignature("getReadableDatabase", "android.database.sqlite.SQLiteDatabase", 23));
        sqliteHelperAPI.Methods.Add(new MethodSignature("onCreate", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("db", "android.database.sqlite.SQLiteDatabase") }
        });
        sqliteHelperAPI.Methods.Add(new MethodSignature("onUpgrade", "void", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("db", "android.database.sqlite.SQLiteDatabase"),
                new("oldVersion", "int"),
                new("newVersion", "int")
            }
        });
        bundle.APIs.Add(sqliteHelperAPI);

        // Cursor
        var cursorAPI = new APIDefinition("android.database.Cursor", 23);
        cursorAPI.Methods.Add(new MethodSignature("moveToFirst", "boolean", 23));
        cursorAPI.Methods.Add(new MethodSignature("moveToNext", "boolean", 23));
        cursorAPI.Methods.Add(new MethodSignature("getString", "String", 23)
        {
            Parameters = new List<ParameterInfo> { new("columnIndex", "int") }
        });
        cursorAPI.Methods.Add(new MethodSignature("getInt", "int", 23)
        {
            Parameters = new List<ParameterInfo> { new("columnIndex", "int") }
        });
        cursorAPI.Methods.Add(new MethodSignature("getLong", "long", 23)
        {
            Parameters = new List<ParameterInfo> { new("columnIndex", "int") }
        });
        cursorAPI.Methods.Add(new MethodSignature("getColumnIndex", "int", 23)
        {
            Parameters = new List<ParameterInfo> { new("columnName", "String") }
        });
        cursorAPI.Methods.Add(new MethodSignature("close", "void", 23));
        bundle.APIs.Add(cursorAPI);
    }

    private static void LoadFileIOAPIs(SDKBundle bundle, int apiLevel)
    {
        // File
        var fileAPI = new APIDefinition("java.io.File", 23);
        fileAPI.Methods.Add(new MethodSignature("exists", "boolean", 23));
        fileAPI.Methods.Add(new MethodSignature("isDirectory", "boolean", 23));
        fileAPI.Methods.Add(new MethodSignature("isFile", "boolean", 23));
        fileAPI.Methods.Add(new MethodSignature("getName", "String", 23));
        fileAPI.Methods.Add(new MethodSignature("getPath", "String", 23));
        fileAPI.Methods.Add(new MethodSignature("getAbsolutePath", "String", 23));
        fileAPI.Methods.Add(new MethodSignature("mkdir", "boolean", 23));
        fileAPI.Methods.Add(new MethodSignature("mkdirs", "boolean", 23));
        fileAPI.Methods.Add(new MethodSignature("delete", "boolean", 23));
        fileAPI.Methods.Add(new MethodSignature("renameTo", "boolean", 23)
        {
            Parameters = new List<ParameterInfo> { new("dest", "java.io.File") }
        });
        fileAPI.Methods.Add(new MethodSignature("listFiles", "java.io.File[]", 23));
        fileAPI.Methods.Add(new MethodSignature("length", "long", 23));
        bundle.APIs.Add(fileAPI);

        // FileInputStream
        var fileInputStreamAPI = new APIDefinition("java.io.FileInputStream", 23);
        fileInputStreamAPI.Methods.Add(new MethodSignature("read", "int", 23));
        fileInputStreamAPI.Methods.Add(new MethodSignature("read", "int", 23)
        {
            Parameters = new List<ParameterInfo> { new("b", "byte[]") }
        });
        fileInputStreamAPI.Methods.Add(new MethodSignature("close", "void", 23));
        bundle.APIs.Add(fileInputStreamAPI);

        // FileOutputStream
        var fileOutputStreamAPI = new APIDefinition("java.io.FileOutputStream", 23);
        fileOutputStreamAPI.Methods.Add(new MethodSignature("write", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("b", "int") }
        });
        fileOutputStreamAPI.Methods.Add(new MethodSignature("write", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("b", "byte[]") }
        });
        fileOutputStreamAPI.Methods.Add(new MethodSignature("flush", "void", 23));
        fileOutputStreamAPI.Methods.Add(new MethodSignature("close", "void", 23));
        bundle.APIs.Add(fileOutputStreamAPI);

        // BufferedReader
        var bufferedReaderAPI = new APIDefinition("java.io.BufferedReader", 23);
        bufferedReaderAPI.Methods.Add(new MethodSignature("readLine", "String", 23));
        bufferedReaderAPI.Methods.Add(new MethodSignature("close", "void", 23));
        bundle.APIs.Add(bufferedReaderAPI);

        // BufferedWriter
        var bufferedWriterAPI = new APIDefinition("java.io.BufferedWriter", 23);
        bufferedWriterAPI.Methods.Add(new MethodSignature("write", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("str", "String") }
        });
        bufferedWriterAPI.Methods.Add(new MethodSignature("newLine", "void", 23));
        bufferedWriterAPI.Methods.Add(new MethodSignature("flush", "void", 23));
        bufferedWriterAPI.Methods.Add(new MethodSignature("close", "void", 23));
        bundle.APIs.Add(bufferedWriterAPI);

        // Environment (for external storage)
        var environmentAPI = new APIDefinition("android.os.Environment", 23);
        environmentAPI.Methods.Add(new MethodSignature("getExternalStorageDirectory", "java.io.File", 23) { IsStatic = true });
        environmentAPI.Methods.Add(new MethodSignature("getExternalStorageState", "String", 23) { IsStatic = true });
        environmentAPI.Fields.Add(new FieldSignature("MEDIA_MOUNTED", "String", 23) { IsStatic = true });
        bundle.APIs.Add(environmentAPI);
    }

    private static void LoadVersionSpecificAPIs(SDKBundle bundle, int apiLevel)
    {
        // Add newer APIs for higher SDK levels
        if (apiLevel >= 28) // Android 9.0
        {
            var biometricAPI = new APIDefinition("android.hardware.biometrics.BiometricPrompt", 28);
            biometricAPI.Methods.Add(new MethodSignature("authenticate", "void", 28)
            {
                Parameters = new List<ParameterInfo> { new("crypto", "android.hardware.biometrics.BiometricPrompt.CryptoObject") }
            });
            bundle.APIs.Add(biometricAPI);
        }
        
        if (apiLevel >= 30) // Android 11.0
        {
            var windowInsetsAPI = new APIDefinition("android.view.WindowInsets", 30);
            windowInsetsAPI.Methods.Add(new MethodSignature("getInsets", "android.graphics.Insets", 30)
            {
                Parameters = new List<ParameterInfo> { new("typeMask", "int") }
            });
            bundle.APIs.Add(windowInsetsAPI);
        }
    }
}
