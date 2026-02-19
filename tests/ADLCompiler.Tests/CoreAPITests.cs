using ADLCompiler.SemanticAnalysis;
using Xunit;

namespace ADLCompiler.Tests;

public class CoreAPITests
{
    [Fact]
    public void APIRegistry_LoadsViewGroupAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var viewGroupAPI = bundle.LookupClass("android.view.ViewGroup");
        Assert.NotNull(viewGroupAPI);
        
        var addViewMethod = bundle.LookupMethod("android.view.ViewGroup", "addView");
        Assert.NotNull(addViewMethod);
        Assert.Equal("void", addViewMethod.ReturnType);
        Assert.Single(addViewMethod.Parameters);
    }

    [Fact]
    public void APIRegistry_LoadsLayoutManagerAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // LinearLayout
        var linearLayoutAPI = bundle.LookupClass("android.widget.LinearLayout");
        Assert.NotNull(linearLayoutAPI);
        var setOrientationMethod = bundle.LookupMethod("android.widget.LinearLayout", "setOrientation");
        Assert.NotNull(setOrientationMethod);
        
        // RelativeLayout
        var relativeLayoutAPI = bundle.LookupClass("android.widget.RelativeLayout");
        Assert.NotNull(relativeLayoutAPI);
        
        // FrameLayout
        var frameLayoutAPI = bundle.LookupClass("android.widget.FrameLayout");
        Assert.NotNull(frameLayoutAPI);
        
        // ConstraintLayout
        var constraintLayoutAPI = bundle.LookupClass("androidx.constraintlayout.widget.ConstraintLayout");
        Assert.NotNull(constraintLayoutAPI);
    }


    [Fact]
    public void APIRegistry_LoadsUIComponentAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // EditText
        var editTextAPI = bundle.LookupClass("android.widget.EditText");
        Assert.NotNull(editTextAPI);
        var setHintMethod = bundle.LookupMethod("android.widget.EditText", "setHint");
        Assert.NotNull(setHintMethod);
        
        // ImageView
        var imageViewAPI = bundle.LookupClass("android.widget.ImageView");
        Assert.NotNull(imageViewAPI);
        var setImageResourceMethod = bundle.LookupMethod("android.widget.ImageView", "setImageResource");
        Assert.NotNull(setImageResourceMethod);
        
        // RecyclerView
        var recyclerViewAPI = bundle.LookupClass("androidx.recyclerview.widget.RecyclerView");
        Assert.NotNull(recyclerViewAPI);
        
        // ListView
        var listViewAPI = bundle.LookupClass("android.widget.ListView");
        Assert.NotNull(listViewAPI);
        
        // ScrollView
        var scrollViewAPI = bundle.LookupClass("android.widget.ScrollView");
        Assert.NotNull(scrollViewAPI);
    }

    [Fact]
    public void APIRegistry_LoadsSharedPreferencesAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // SharedPreferences
        var sharedPrefsAPI = bundle.LookupClass("android.content.SharedPreferences");
        Assert.NotNull(sharedPrefsAPI);
        var getStringMethod = bundle.LookupMethod("android.content.SharedPreferences", "getString");
        Assert.NotNull(getStringMethod);
        Assert.Equal("String", getStringMethod.ReturnType);
        Assert.Equal(2, getStringMethod.Parameters.Count);
        
        // SharedPreferences.Editor
        var editorAPI = bundle.LookupClass("android.content.SharedPreferences.Editor");
        Assert.NotNull(editorAPI);
        var putStringMethod = bundle.LookupMethod("android.content.SharedPreferences.Editor", "putString");
        Assert.NotNull(putStringMethod);
        var commitMethod = bundle.LookupMethod("android.content.SharedPreferences.Editor", "commit");
        Assert.NotNull(commitMethod);
        Assert.Equal("boolean", commitMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsSQLiteAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // SQLiteDatabase
        var sqliteAPI = bundle.LookupClass("android.database.sqlite.SQLiteDatabase");
        Assert.NotNull(sqliteAPI);
        var execSQLMethod = bundle.LookupMethod("android.database.sqlite.SQLiteDatabase", "execSQL");
        Assert.NotNull(execSQLMethod);
        var insertMethod = bundle.LookupMethod("android.database.sqlite.SQLiteDatabase", "insert");
        Assert.NotNull(insertMethod);
        Assert.Equal("long", insertMethod.ReturnType);
        
        // SQLiteOpenHelper
        var helperAPI = bundle.LookupClass("android.database.sqlite.SQLiteOpenHelper");
        Assert.NotNull(helperAPI);
        var getWritableDBMethod = bundle.LookupMethod("android.database.sqlite.SQLiteOpenHelper", "getWritableDatabase");
        Assert.NotNull(getWritableDBMethod);
        
        // Cursor
        var cursorAPI = bundle.LookupClass("android.database.Cursor");
        Assert.NotNull(cursorAPI);
        var moveToFirstMethod = bundle.LookupMethod("android.database.Cursor", "moveToFirst");
        Assert.NotNull(moveToFirstMethod);
    }

    [Fact]
    public void APIRegistry_LoadsFileIOAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // File
        var fileAPI = bundle.LookupClass("java.io.File");
        Assert.NotNull(fileAPI);
        var existsMethod = bundle.LookupMethod("java.io.File", "exists");
        Assert.NotNull(existsMethod);
        Assert.Equal("boolean", existsMethod.ReturnType);
        
        // FileInputStream
        var fileInputStreamAPI = bundle.LookupClass("java.io.FileInputStream");
        Assert.NotNull(fileInputStreamAPI);
        
        // FileOutputStream
        var fileOutputStreamAPI = bundle.LookupClass("java.io.FileOutputStream");
        Assert.NotNull(fileOutputStreamAPI);
        
        // BufferedReader
        var bufferedReaderAPI = bundle.LookupClass("java.io.BufferedReader");
        Assert.NotNull(bufferedReaderAPI);
        var readLineMethod = bundle.LookupMethod("java.io.BufferedReader", "readLine");
        Assert.NotNull(readLineMethod);
        
        // BufferedWriter
        var bufferedWriterAPI = bundle.LookupClass("java.io.BufferedWriter");
        Assert.NotNull(bufferedWriterAPI);
        
        // Environment
        var environmentAPI = bundle.LookupClass("android.os.Environment");
        Assert.NotNull(environmentAPI);
        var getExternalStorageMethod = bundle.LookupMethod("android.os.Environment", "getExternalStorageDirectory");
        Assert.NotNull(getExternalStorageMethod);
    }

    [Fact]
    public void APIRegistry_LoadsContextAPIsWithFileOperations()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        var contextAPI = bundle.LookupClass("android.content.Context");
        Assert.NotNull(contextAPI);
        
        // File operations
        var getFilesDirMethod = bundle.LookupMethod("android.content.Context", "getFilesDir");
        Assert.NotNull(getFilesDirMethod);
        Assert.Equal("java.io.File", getFilesDirMethod.ReturnType);
        
        var getCacheDirMethod = bundle.LookupMethod("android.content.Context", "getCacheDir");
        Assert.NotNull(getCacheDirMethod);
        
        var openFileInputMethod = bundle.LookupMethod("android.content.Context", "openFileInput");
        Assert.NotNull(openFileInputMethod);
        Assert.Equal("java.io.FileInputStream", openFileInputMethod.ReturnType);
        
        var openFileOutputMethod = bundle.LookupMethod("android.content.Context", "openFileOutput");
        Assert.NotNull(openFileOutputMethod);
        Assert.Equal("java.io.FileOutputStream", openFileOutputMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsIntentAPIsWithExtras()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        var intentAPI = bundle.LookupClass("android.content.Intent");
        Assert.NotNull(intentAPI);
        
        // Extra methods
        var getIntExtraMethod = bundle.LookupMethod("android.content.Intent", "getIntExtra");
        Assert.NotNull(getIntExtraMethod);
        Assert.Equal(2, getIntExtraMethod.Parameters.Count);
        
        var getBooleanExtraMethod = bundle.LookupMethod("android.content.Intent", "getBooleanExtra");
        Assert.NotNull(getBooleanExtraMethod);
        
        // Action methods
        var setActionMethod = bundle.LookupMethod("android.content.Intent", "setAction");
        Assert.NotNull(setActionMethod);
        
        var getActionMethod = bundle.LookupMethod("android.content.Intent", "getAction");
        Assert.NotNull(getActionMethod);
        
        // Fields
        var actionViewField = bundle.LookupField("android.content.Intent", "ACTION_VIEW");
        Assert.NotNull(actionViewField);
        Assert.True(actionViewField.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsViewAPIsWithLayoutMethods()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        var viewAPI = bundle.LookupClass("android.view.View");
        Assert.NotNull(viewAPI);
        
        // Layout methods
        var setPaddingMethod = bundle.LookupMethod("android.view.View", "setPadding");
        Assert.NotNull(setPaddingMethod);
        Assert.Equal(4, setPaddingMethod.Parameters.Count);
        
        var getWidthMethod = bundle.LookupMethod("android.view.View", "getWidth");
        Assert.NotNull(getWidthMethod);
        Assert.Equal("int", getWidthMethod.ReturnType);
        
        var getHeightMethod = bundle.LookupMethod("android.view.View", "getHeight");
        Assert.NotNull(getHeightMethod);
    }
}
