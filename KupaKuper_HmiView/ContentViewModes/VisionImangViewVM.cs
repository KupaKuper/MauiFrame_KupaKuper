using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KupaKuper_HmiView.Resources;

using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace KupaKuper_HmiView.ContentViewModes;

public partial class VisionImangViewVM : BaseViewMode
{
    private static string? PhotoAdr;
    // 文件夹结构的集合
    [ObservableProperty]
    private ObservableCollection<FolderItem> folderStructure;

    // 当前选中的文件夹
    [ObservableProperty]
    private FolderItem selectedFolder;

    // 当前显示的图片
    [ObservableProperty]
    private ImageSource currentImage;

    // 当前图片的名称
    [ObservableProperty]
    private string currentImageName;

    // 是否没有图片
    [ObservableProperty]
    private bool isNoImage = true;

    // 图片项的集合
    [ObservableProperty]
    private ObservableCollection<ImageItem> imageItems;

    // 当前选中的图片项
    [ObservableProperty]
    private ImageItem selectedImageItem;

    // 当前图片的缩放比例
    [ObservableProperty]
    private double currentScale = 1.0;

    // 当前图片的创建时间
    [ObservableProperty]
    private string currentImageCreateTime;

    // 当前图片的修改时间
    [ObservableProperty]
    private string currentImageModifyTime;

    // 当前图片的大小
    [ObservableProperty]
    private string currentImageSize;

    // 当前图片的路径
    [ObservableProperty]
    private string currentImagePath;

    // 是否可以导航图片
    [ObservableProperty]
    private bool canNavigateImages;

    // 上一张图片的提示
    [ObservableProperty]
    private string previousImageTooltip;

    // 下一张图片的提示
    [ObservableProperty]
    private string nextImageTooltip;

    // 上一张按钮的背景色
    [ObservableProperty]
    private Color previousButtonBackground = Colors.Transparent;

    // 下一张按钮的背景色
    [ObservableProperty]
    private Color nextButtonBackground = Colors.Transparent;

    // 默认按钮颜色
    private readonly Color defaultButtonColor = Colors.Transparent;

    // 悬停按钮颜色
    private readonly Color hoverButtonColor = Colors.LightGray;

    // 当前图片文件列表
    private List<string> currentImageFiles;

    // 当前图片索引
    private int currentImageIndex = -1;

    public override uint ViewIndex { get; set; }
    public override string ViewName { get => AppResources.ResourceManager?.GetString("生产图片") ?? "生产图片"; set => throw new NotImplementedException(); }

    // 构造函数
    public VisionImangViewVM()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS) return;
        PhotoAdr = Device.dataConfig.PictureAdr;
        folderStructure = new ObservableCollection<FolderItem>();
        imageItems = new ObservableCollection<ImageItem>();
        System.Diagnostics.Debug.WriteLine("ViewModel 初始化开始");
        Task.Run(LoadFolderStructure);
    }
    /// <summary>
    /// 读取文件内的图片
    /// </summary>
    private async void LoadData()
    {
        if (NowView != ViewIndex) return;
        await LoadFolderStructure();
    }
    // 加载文件夹结构
    private async Task LoadFolderStructure()
    {
        try
        {
            string baseFolder = PhotoAdr;
            System.Diagnostics.Debug.WriteLine($"正在检查文件夹路径: {baseFolder}");

            if (!Directory.Exists(baseFolder))
            {
                System.Diagnostics.Debug.WriteLine($"文件夹不存在: {baseFolder}");
                return;
            }

            var rootFolders = Directory.GetDirectories(baseFolder);
            System.Diagnostics.Debug.WriteLine($"找到文件夹数量: {rootFolders.Length}");

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                FolderStructure.Clear();
                foreach (var folder in rootFolders)
                {
                    var folderItem = new FolderItem
                    {
                        Name = Path.GetFileName(folder),
                        Path = folder
                    };
                    FolderStructure.Add(folderItem);
                    System.Diagnostics.Debug.WriteLine($"添加文件夹: {folderItem.Name}");
                }

                // 如果有文件夹，选择第一个
                if (FolderStructure.Any())
                {
                    SelectedFolder = FolderStructure[0];
                    System.Diagnostics.Debug.WriteLine($"选择第一个文件夹: {selectedFolder.Name}");
                    LoadImagesFromFolder(SelectedFolder.Path);
                }
                else
                {
                    // 如果没有子文件夹，直接加载根目录的图片
                    System.Diagnostics.Debug.WriteLine("没有找到子文件夹，尝试加载根目录图片");
                    LoadImagesFromFolder(baseFolder);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载文件夹结构时出错: {ex}");
        }
    }

    // 从文件夹加载图片
    private async void LoadImagesFromFolder(string folderPath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"开始加载文件夹图片: {folderPath}");

            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            currentImageFiles = Directory.GetFiles(folderPath)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"找到图片文件数量: {currentImageFiles.Count}");

            // 更新图片列表
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ImageItems.Clear();
                foreach (var file in currentImageFiles)
                {
                    ImageItems.Add(new ImageItem
                    {
                        Name = Path.GetFileName(file),
                        Path = file
                    });
                }
            });

            if (currentImageFiles.Any())
            {
                currentImageIndex = 0;
                CanNavigateImages = currentImageFiles.Count > 1;
                await LoadCurrentImage();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsNoImage = true;
                    CanNavigateImages = false;
                    CurrentImage = null;
                    CurrentImageName = "没有找到图片文件";
                    OnPropertyChanged(nameof(IsNoImage));
                    OnPropertyChanged(nameof(CanNavigateImages));
                    OnPropertyChanged(nameof(CurrentImage));
                    OnPropertyChanged(nameof(CurrentImageName));
                    PreviousButtonBackground = defaultButtonColor;
                    NextButtonBackground = defaultButtonColor;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载图片文件夹时出错: {ex}");
        }
    }

    // 加载当前图片
    private async Task LoadCurrentImage()
    {
        try
        {
            if (currentImageIndex >= 0 && currentImageIndex < currentImageFiles.Count)
            {
                var imagePath = currentImageFiles[currentImageIndex];
                System.Diagnostics.Debug.WriteLine($"正在加载图片: {imagePath}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // 获取文件信息
                        var fileInfo = new FileInfo(imagePath);
                        CurrentImageCreateTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                        CurrentImageModifyTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                        CurrentImageSize = FormatFileSize(fileInfo.Length);
                        CurrentImagePath = imagePath;

                        // 加载图片并调整初始缩放
                        using var stream = File.OpenRead(imagePath);
                        var imageData = new byte[stream.Length];
                        await stream.ReadExactlyAsync(imageData.AsMemory(0, (int)stream.Length));

                        // 创建图片源
                        CurrentImage = ImageSource.FromStream(() => new MemoryStream(imageData));
                        CurrentImageName = Path.GetFileName(imagePath);
                        IsNoImage = false;

                        // 更新所有属性
                        OnPropertyChanged(nameof(CurrentImage));
                        OnPropertyChanged(nameof(CurrentImageName));
                        OnPropertyChanged(nameof(CurrentImagePath));
                        OnPropertyChanged(nameof(CurrentImageCreateTime));
                        OnPropertyChanged(nameof(CurrentImageModifyTime));
                        OnPropertyChanged(nameof(CurrentImageSize));
                        OnPropertyChanged(nameof(IsNoImage));

                        // 更新导航提示
                        UpdateNavigationTooltips();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"在UI线程加载图片时出错: {ex}");
                        ResetImageInfo();
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载图片时出错: {ex}");
            ResetImageInfo();
        }
    }

    // 重置图片信息
    private void ResetImageInfo()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentImage = null;
            CurrentImageName = "图片加载失败";
            CurrentImagePath = "-";
            CurrentImageCreateTime = "-";
            CurrentImageModifyTime = "-";
            CurrentImageSize = "-";
            IsNoImage = true;

            OnPropertyChanged(nameof(CurrentImage));
            OnPropertyChanged(nameof(CurrentImageName));
            OnPropertyChanged(nameof(CurrentImagePath));
            OnPropertyChanged(nameof(CurrentImageCreateTime));
            OnPropertyChanged(nameof(CurrentImageModifyTime));
            OnPropertyChanged(nameof(CurrentImageSize));
            OnPropertyChanged(nameof(IsNoImage));
        });
    }

    // 格式化文件大小
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    // 选中文件夹改变时的处理
    partial void OnSelectedFolderChanged(FolderItem value)
    {
        if (value != null)
        {
            System.Diagnostics.Debug.WriteLine($"文件夹选择改变: {value.Name}");
            LoadImagesFromFolder(value.Path);
        }
    }

    partial void OnSelectedImageItemChanged(ImageItem value)
    {
        if (value != null)
        {
            var index = currentImageFiles.IndexOf(value.Path);
            if (index != -1)
            {
                currentImageIndex = index;
                LoadCurrentImage().ConfigureAwait(false);
            }
        }
    }

    // 上一张图片命令
    [RelayCommand]
    private async void PreviousImage()
    {
        System.Diagnostics.Debug.WriteLine("点击上一张按钮");
        if (currentImageFiles == null || !currentImageFiles.Any())
        {
            System.Diagnostics.Debug.WriteLine("没有可用的图片文件");
            return;
        }

        currentImageIndex--;
        if (currentImageIndex < 0)
            currentImageIndex = currentImageFiles.Count - 1;

        System.Diagnostics.Debug.WriteLine($"切换到上一张图片，索引: {currentImageIndex}");
        await LoadCurrentImage();
    }

    // 下一张图片命令
    [RelayCommand]
    private async void NextImage()
    {
        System.Diagnostics.Debug.WriteLine("点击下一张按钮");
        if (currentImageFiles == null || !currentImageFiles.Any())
        {
            System.Diagnostics.Debug.WriteLine("没有可用的图片文件");
            return;
        }

        currentImageIndex++;
        if (currentImageIndex >= currentImageFiles.Count)
            currentImageIndex = 0;

        System.Diagnostics.Debug.WriteLine($"切换到下一张图片，索引: {currentImageIndex}");
        await LoadCurrentImage();
    }

    // 处理捏合手势更新
    public void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        try
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    break;
                case GestureStatus.Running:
                    CurrentScale = Math.Max(0.1, Math.Min(3.0, CurrentScale * e.Scale));
                    break;
                case GestureStatus.Completed:
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnPinchUpdated error: {ex.Message}");
        }
    }

    // 打开文件位置命令
    [RelayCommand]
    private void OpenFileLocation()
    {
        try
        {
            if (!string.IsNullOrEmpty(CurrentImagePath) && File.Exists(CurrentImagePath))
            {
                // 打开文件所在文件夹并选中文件
                Process.Start("explorer.exe", $"/select,\"{CurrentImagePath}\"");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开文件位置时出错: {ex}");
        }
    }

    // 更新缩放比例
    public void UpdateScale(double scaleChange)
    {
        try
        {
            var newScale = CurrentScale * scaleChange;
            // 限制缩放范围
            CurrentScale = Math.Max(0.1, Math.Min(3.0, newScale));
            OnPropertyChanged(nameof(CurrentScale));
            System.Diagnostics.Debug.WriteLine($"缩放更新: {CurrentScale}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"更新缩放时出错: {ex}");
        }
    }

    // 处理滚动事件
    public void HandleScroll(double delta)
    {
        try
        {
            var scaleChange = delta > 0 ? 0.9 : 1.1;
            CurrentScale = Math.Max(0.1, Math.Min(3.0, CurrentScale * scaleChange));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HandleScroll error: {ex.Message}");
        }
    }

    // 是否没有图片改变时的处理
    partial void OnIsNoImageChanged(bool value)
    {
        CanNavigateImages = !value && (currentImageFiles?.Count > 1 ? false : true);
    }

    // 更新导航提示
    private void UpdateNavigationTooltips()
    {
        try
        {
            if (currentImageFiles == null || !currentImageFiles.Any())
            {
                PreviousImageTooltip = "没有可用的图片";
                NextImageTooltip = "没有可用的图片";
                return;
            }

            int previousIndex = currentImageIndex - 1;
            if (previousIndex < 0) previousIndex = currentImageFiles.Count - 1;

            int nextIndex = currentImageIndex + 1;
            if (nextIndex >= currentImageFiles.Count) nextIndex = 0;

            PreviousImageTooltip = $"上一张: {Path.GetFileName(currentImageFiles[previousIndex])}";
            NextImageTooltip = $"下一张: {Path.GetFileName(currentImageFiles[nextIndex])}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"更新导航提示时出错: {ex}");
            PreviousImageTooltip = "上一张";
            NextImageTooltip = "下一张";
        }
    }

    // 上一张按钮悬停命令
    [RelayCommand]
    private void PreviousButtonHover(bool isHovered)
    {
        try
        {
            if (currentImageFiles == null || !currentImageFiles.Any())
            {
                return;
            }
            PreviousButtonBackground = isHovered ? hoverButtonColor : defaultButtonColor;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"更新上一张按钮背景色时出错: {ex}");
        }
    }

    // 下一张按钮悬停命令
    [RelayCommand]
    private void NextButtonHover(bool isHovered)
    {
        try
        {
            if (currentImageFiles == null || !currentImageFiles.Any())
            {
                return;
            }
            NextButtonBackground = isHovered ? hoverButtonColor : defaultButtonColor;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"更新下一张按钮背景色时出错: {ex}");
        }
    }

    public override void UpdataView()
    {
        throw new NotImplementedException();
    }

    public override void OnViewVisible()
    {
        base.NowView = this.ViewIndex;
    }

    public override void CloseViewVisible()
    {
        throw new NotImplementedException();
    }
}

// 文件夹项类
public class FolderItem
{
    public string Name { get; set; }
    public string Path { get; set; }
}

// 图片项类
public class ImageItem
{
    public string Name { get; set; }
    public string Path { get; set; }
}
