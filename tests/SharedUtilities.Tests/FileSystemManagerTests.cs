using SharedUtilities.FileSystem;
using Xunit;

namespace SharedUtilities.Tests;

public class FileSystemManagerTests
{
    [Fact]
    public void GetRootDirectory_ReturnsValidPath()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        
        // Act
        var rootDir = fileSystem.GetRootDirectory();
        
        // Assert
        Assert.NotNull(rootDir);
        Assert.NotEmpty(rootDir);
    }
    
    [Fact]
    public void ResolvePath_WithTilde_ExpandsToRootDirectory()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var rootDir = fileSystem.GetRootDirectory();
        
        // Act
        var resolved = fileSystem.ResolvePath("~");
        
        // Assert
        Assert.Equal(rootDir, resolved);
    }
    
    [Fact]
    public void ResolvePath_WithTildeAndPath_ExpandsCorrectly()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var rootDir = fileSystem.GetRootDirectory();
        
        // Act
        var resolved = fileSystem.ResolvePath("~/projects/myapp");
        
        // Assert
        Assert.StartsWith(rootDir, resolved);
        Assert.Contains("projects", resolved);
        Assert.Contains("myapp", resolved);
    }
    
    [Fact]
    public void ResolvePath_WithRelativePath_MakesAbsolute()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var rootDir = fileSystem.GetRootDirectory();
        
        // Act
        var resolved = fileSystem.ResolvePath("projects/myapp");
        
        // Assert
        Assert.StartsWith(rootDir, resolved);
        Assert.Contains("projects", resolved);
    }
    
    [Fact]
    public void CreateDirectory_CreatesDirectory()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var testDir = $"~/test-{Guid.NewGuid()}";
        
        try
        {
            // Act
            fileSystem.CreateDirectory(testDir);
            
            // Assert
            Assert.True(fileSystem.DirectoryExists(testDir));
        }
        finally
        {
            // Cleanup
            if (fileSystem.DirectoryExists(testDir))
            {
                fileSystem.DeleteDirectory(testDir, recursive: true);
            }
        }
    }
    
    [Fact]
    public void WriteAllText_CreatesFileWithContent()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var testFile = $"~/test-{Guid.NewGuid()}.txt";
        var content = "Hello, World!";
        
        try
        {
            // Act
            fileSystem.WriteAllText(testFile, content);
            
            // Assert
            Assert.True(fileSystem.FileExists(testFile));
            var readContent = fileSystem.ReadAllText(testFile);
            Assert.Equal(content, readContent);
        }
        finally
        {
            // Cleanup
            if (fileSystem.FileExists(testFile))
            {
                fileSystem.DeleteFile(testFile);
            }
        }
    }
    
    [Fact]
    public void CombinePath_CombinesPathSegments()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        
        // Act
        var combined = fileSystem.CombinePath("projects", "myapp", "src");
        
        // Assert
        Assert.Contains("projects", combined);
        Assert.Contains("myapp", combined);
        Assert.Contains("src", combined);
    }
    
    [Fact]
    public void EnsureRootDirectoryExists_CreatesRootDirectory()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        
        // Act
        fileSystem.EnsureRootDirectoryExists();
        
        // Assert
        var rootDir = fileSystem.GetRootDirectory();
        Assert.True(Directory.Exists(rootDir));
    }
    
    [Fact]
    public void CopyFile_CopiesFileSuccessfully()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var sourceFile = $"~/test-source-{Guid.NewGuid()}.txt";
        var destFile = $"~/test-dest-{Guid.NewGuid()}.txt";
        var content = "Test content for copy";
        
        try
        {
            fileSystem.WriteAllText(sourceFile, content);
            
            // Act
            fileSystem.CopyFile(sourceFile, destFile);
            
            // Assert
            Assert.True(fileSystem.FileExists(destFile));
            var copiedContent = fileSystem.ReadAllText(destFile);
            Assert.Equal(content, copiedContent);
        }
        finally
        {
            // Cleanup
            if (fileSystem.FileExists(sourceFile))
                fileSystem.DeleteFile(sourceFile);
            if (fileSystem.FileExists(destFile))
                fileSystem.DeleteFile(destFile);
        }
    }
    
    [Fact]
    public void CopyDirectory_CopiesDirectoryRecursively()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var sourceDir = $"~/test-source-dir-{Guid.NewGuid()}";
        var destDir = $"~/test-dest-dir-{Guid.NewGuid()}";
        
        try
        {
            // Create source directory structure
            fileSystem.CreateDirectory(sourceDir);
            fileSystem.WriteAllText($"{sourceDir}/file1.txt", "Content 1");
            fileSystem.CreateDirectory($"{sourceDir}/subdir");
            fileSystem.WriteAllText($"{sourceDir}/subdir/file2.txt", "Content 2");
            
            // Act
            fileSystem.CopyDirectory(sourceDir, destDir, recursive: true);
            
            // Assert
            Assert.True(fileSystem.DirectoryExists(destDir));
            Assert.True(fileSystem.FileExists($"{destDir}/file1.txt"));
            Assert.True(fileSystem.DirectoryExists($"{destDir}/subdir"));
            Assert.True(fileSystem.FileExists($"{destDir}/subdir/file2.txt"));
            
            var content1 = fileSystem.ReadAllText($"{destDir}/file1.txt");
            var content2 = fileSystem.ReadAllText($"{destDir}/subdir/file2.txt");
            Assert.Equal("Content 1", content1);
            Assert.Equal("Content 2", content2);
        }
        finally
        {
            // Cleanup
            if (fileSystem.DirectoryExists(sourceDir))
                fileSystem.DeleteDirectory(sourceDir, recursive: true);
            if (fileSystem.DirectoryExists(destDir))
                fileSystem.DeleteDirectory(destDir, recursive: true);
        }
    }
    
    [Fact]
    public void MoveFile_MovesFileSuccessfully()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var sourceFile = $"~/test-move-source-{Guid.NewGuid()}.txt";
        var destFile = $"~/test-move-dest-{Guid.NewGuid()}.txt";
        var content = "Test content for move";
        
        try
        {
            fileSystem.WriteAllText(sourceFile, content);
            
            // Act
            fileSystem.MoveFile(sourceFile, destFile);
            
            // Assert
            Assert.False(fileSystem.FileExists(sourceFile));
            Assert.True(fileSystem.FileExists(destFile));
            var movedContent = fileSystem.ReadAllText(destFile);
            Assert.Equal(content, movedContent);
        }
        finally
        {
            // Cleanup
            if (fileSystem.FileExists(sourceFile))
                fileSystem.DeleteFile(sourceFile);
            if (fileSystem.FileExists(destFile))
                fileSystem.DeleteFile(destFile);
        }
    }
    
    [Fact]
    public void MoveDirectory_MovesDirectorySuccessfully()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var sourceDir = $"~/test-move-source-dir-{Guid.NewGuid()}";
        var destDir = $"~/test-move-dest-dir-{Guid.NewGuid()}";
        
        try
        {
            // Create source directory with content
            fileSystem.CreateDirectory(sourceDir);
            fileSystem.WriteAllText($"{sourceDir}/file.txt", "Content");
            
            // Act
            fileSystem.MoveDirectory(sourceDir, destDir);
            
            // Assert
            Assert.False(fileSystem.DirectoryExists(sourceDir));
            Assert.True(fileSystem.DirectoryExists(destDir));
            Assert.True(fileSystem.FileExists($"{destDir}/file.txt"));
        }
        finally
        {
            // Cleanup
            if (fileSystem.DirectoryExists(sourceDir))
                fileSystem.DeleteDirectory(sourceDir, recursive: true);
            if (fileSystem.DirectoryExists(destDir))
                fileSystem.DeleteDirectory(destDir, recursive: true);
        }
    }
    
    [Fact]
    public void CreateWatcher_WatchesFileChanges()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var testDir = $"~/test-watch-{Guid.NewGuid()}";
        fileSystem.CreateDirectory(testDir);
        
        var createdEventFired = false;
        var changedEventFired = false;
        var deletedEventFired = false;
        
        try
        {
            using var watcher = fileSystem.CreateWatcher(testDir, "*.txt");
            
            watcher.Created += (sender, e) => createdEventFired = true;
            watcher.Changed += (sender, e) => changedEventFired = true;
            watcher.Deleted += (sender, e) => deletedEventFired = true;
            
            watcher.EnableRaisingEvents = true;
            
            // Act
            var testFile = $"{testDir}/test.txt";
            fileSystem.WriteAllText(testFile, "Initial content");
            Thread.Sleep(100); // Give watcher time to detect
            
            fileSystem.WriteAllText(testFile, "Modified content");
            Thread.Sleep(100); // Give watcher time to detect
            
            fileSystem.DeleteFile(testFile);
            Thread.Sleep(100); // Give watcher time to detect
            
            // Assert
            Assert.True(createdEventFired, "Created event should have fired");
            Assert.True(changedEventFired, "Changed event should have fired");
            Assert.True(deletedEventFired, "Deleted event should have fired");
        }
        finally
        {
            // Cleanup
            if (fileSystem.DirectoryExists(testDir))
                fileSystem.DeleteDirectory(testDir, recursive: true);
        }
    }
    
    [Fact]
    public void GetFiles_WithRecursive_ReturnsAllFiles()
    {
        // Arrange
        var fileSystem = new FileSystemManager();
        var testDir = $"~/test-getfiles-{Guid.NewGuid()}";
        
        try
        {
            // Create directory structure
            fileSystem.CreateDirectory(testDir);
            fileSystem.WriteAllText($"{testDir}/file1.txt", "Content 1");
            fileSystem.CreateDirectory($"{testDir}/subdir");
            fileSystem.WriteAllText($"{testDir}/subdir/file2.txt", "Content 2");
            
            // Act
            var files = fileSystem.GetFiles(testDir, "*.txt", recursive: true);
            
            // Assert
            Assert.Equal(2, files.Length);
        }
        finally
        {
            // Cleanup
            if (fileSystem.DirectoryExists(testDir))
                fileSystem.DeleteDirectory(testDir, recursive: true);
        }
    }
}
