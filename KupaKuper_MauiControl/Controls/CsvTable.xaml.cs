using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;

namespace KupaKuper_MauiControl.Controls;

public partial class CsvTable : ContentView, IDisposable
{
	#region 绑定属性
	/// <summary>
	/// ViewModel绑定属性，用于数据处理和UI逻辑
	/// </summary>
	public static readonly BindableProperty ViewModelProperty =
		BindableProperty.Create(nameof(ViewModel), typeof(CsvTableViewMode), typeof(CsvTable), null, propertyChanged: OnViewModelChanged);

	/// <summary>
	/// 搜索框可见性绑定属性
	/// </summary>
	public static readonly BindableProperty IsSearchVisibleProperty =
		BindableProperty.Create(nameof(IsSearchVisible), typeof(bool), typeof(CsvTable), true, 
			propertyChanged: OnIsSearchVisibleChanged);

	/// <summary>
	/// 表格是否可编辑绑定属性
	/// </summary>
	public static readonly BindableProperty IsEditableProperty =
		BindableProperty.Create(nameof(IsEditable), typeof(bool), typeof(CsvTable), 
			false, propertyChanged: OnIsEditableChanged);

	/// <summary>
	/// 数据源绑定属性，设置后会触发数据加载
	/// </summary>
	public static readonly BindableProperty DataSourceProperty =
		BindableProperty.Create(nameof(DataSource), typeof(CsvTableDataMode), typeof(CsvTable), 
			null, propertyChanged: OnDataSourceChanged);

	/// <summary>
	/// 文件路径列表绑定属性，用于文件选择器
	/// </summary>
	public static readonly BindableProperty FilePathsProperty =
		BindableProperty.Create(nameof(FilePaths), typeof(List<string>), typeof(CsvTable), 
			null, propertyChanged: OnFilePathsChanged);

	/// <summary>
	/// 文件选择器可见性绑定属性
	/// </summary>
	public static readonly BindableProperty IsPickerVisibleProperty =
		BindableProperty.Create(nameof(IsPickerVisible), typeof(bool), typeof(CsvTable), 
			false, propertyChanged: OnIsPickerVisibleChanged);

	/// <summary>
	/// 当前选中文件路径绑定属性
	/// </summary>
	public static readonly BindableProperty SelectedFilePathProperty =
		BindableProperty.Create(nameof(SelectedFilePath), typeof(string), typeof(CsvTable), 
			null, BindingMode.TwoWay, propertyChanged: OnSelectedFilePathChanged);

	/// <summary>
	/// 数据处理和UI逻辑的ViewModel
	/// </summary>
	public CsvTableViewMode ViewModel
	{
		get => (CsvTableViewMode)GetValue(ViewModelProperty);
		set
		{
			if ((CsvTableViewMode)GetValue(ViewModelProperty) != value)
			{
				SetValue(ViewModelProperty, value);
				if (value != null)
				{
					SetupViewModelEvents();
				}
			}
		}
	}

	/// <summary>
	/// 控制搜索框是否可见
	/// </summary>
	public bool IsSearchVisible
	{
		get => (bool)GetValue(IsSearchVisibleProperty);
		set => SetValue(IsSearchVisibleProperty, value);
	}

	/// <summary>
	/// 控制表格内容是否可编辑
	/// </summary>
	public bool IsEditable
	{
		get => (bool)GetValue(IsEditableProperty);
		set => SetValue(IsEditableProperty, value);
	}

	/// <summary>
	/// 表格数据源
	/// </summary>
	public CsvTableDataMode DataSource
	{
		get => (CsvTableDataMode)GetValue(DataSourceProperty);
		set
		{
			SetValue(DataSourceProperty, value);
			
			// 延迟一帧执行，确保属性已更新
			Dispatcher.Dispatch(() => {
				if (value != null)
				{
					// 确保ViewModel已初始化
					if (ViewModel == null)
					{
						ViewModel = new CsvTableViewMode();
					}
					
					// 直接设置数据源到ViewModel
					ViewModel.SetBindingData(value, Width > 0 ? Width - 20 : 800);
					
					// 直接设置行数据
					TableDrawable.Rows = ViewModel.FilteredRows;
					
					// 更新空视图状态
					OnPropertyChanged(nameof(IsEmptyViewVisible));
					
					// 强制重绘
					tableGraphicsView?.Invalidate();
				}
			});
		}
	}

	/// <summary>
	/// 文件路径列表，用于文件选择器
	/// </summary>
	public List<string> FilePaths
	{
		get => (List<string>)GetValue(FilePathsProperty);
		set => SetValue(FilePathsProperty, value);
	}

	/// <summary>
	/// 控制文件选择器是否可见
	/// </summary>
	public bool IsPickerVisible
	{
		get => (bool)GetValue(IsPickerVisibleProperty);
		set => SetValue(IsPickerVisibleProperty, value);
	}

	/// <summary>
	/// 当前选中的文件路径
	/// </summary>
	public string SelectedFilePath
	{
		get => (string)GetValue(SelectedFilePathProperty);
		set => SetValue(SelectedFilePathProperty, value);
	}

	/// <summary>
	/// 表格为空时显示空视图
	/// </summary>
	public bool IsEmptyViewVisible => ViewModel?.FilteredRows == null || ViewModel?.FilteredRows.Count == 0;

	/// <summary>
	/// 控制表格网格的行位置
	/// </summary>
	public int TableGridRow => (IsSearchVisible || IsPickerVisible) ? 1 : 0;

	/// <summary>
	/// 控制表格网格的行跨度
	/// </summary>
	public int TableGridRowSpan => (IsSearchVisible || IsPickerVisible) ? 1 : 2;

	/// <summary>
	/// 表格绘制对象，负责实际渲染表格内容
	/// </summary>
	public CsvTableGraphicsDrawable TableDrawable { get; private set; } = new CsvTableGraphicsDrawable();

	/// <summary>
	/// 文件名列表，用于Picker显示
	/// </summary>
	public ObservableCollection<string> FileNames { get; private set; } = new ObservableCollection<string>();
	#endregion

	/// <summary>
	/// 文件选择变更事件委托
	/// </summary>
	public delegate void FileSelectedEventHandler(string filePath);

	/// <summary>
	/// 文件选择变更事件
	/// </summary>
	public event FileSelectedEventHandler FileSelected;

	/// <summary>
	/// 文件路径字典（文件名->文件路径）
	/// </summary>
	private Dictionary<string, string> _filePathDict = new Dictionary<string, string>();

	/// <summary>
	/// 待处理的数据源
	/// </summary>
	private CsvTableDataMode _pendingDataSource;

	/// <summary>
	/// 当前垂直滚动位置
	/// </summary>
	private double _scrollY = 0;

	/// <summary>
	/// 当前滚动速度，用于惯性滚动
	/// </summary>
	private double _scrollVelocity = 0;

	/// <summary>
	/// 上次触摸事件的Y坐标
	/// </summary>
	private double _lastTouchY = 0;

	/// <summary>
	/// 指示当前是否正在拖动
	/// </summary>
	private bool _isDragging = false;

	/// <summary>
	/// 滚动计时器，用于处理惯性滚动
	/// </summary>
	private IDispatcherTimer _scrollTimer;

	/// <summary>
	/// 滚动减速因子，值越小减速越快
	/// </summary>
	private double _scrollAcceleration = 0.90;

	/// <summary>
	/// 最小速度阈值，低于此值停止惯性滚动
	/// </summary>
	private double _minVelocityThreshold = 0.5;

	/// <summary>
	/// 鼠标滚轮滚动系数，用于调整滚轮灵敏度
	/// </summary>
	private double _wheelScrollFactor = 10.0;

	/// <summary>
	/// 上次拖动事件的时间戳
	/// </summary>
	private DateTime _lastDragTime;

	/// <summary>
	/// 最后一次有效的拖动速度
	/// </summary>
	private double _lastValidVelocity = 0;

	/// <summary>
	/// 搜索输入框引用
	/// </summary>
	private Entry _searchEntry;

	/// <summary>
	/// 定时器用于定期检查数据源变化
	/// </summary>
	private IDispatcherTimer _dataSourceWatchTimer;

	/// <summary>
	/// 监控数据源属性变化的频率(毫秒)
	/// </summary>
	private const int DATA_SOURCE_WATCH_INTERVAL = 500;

	/// <summary>
	/// 上次数据源哈希值，用于检测变化
	/// </summary>
	private int _lastDataSourceHash = 0;

	/// <summary>
	/// ViewModel变更处理
	/// </summary>
	private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CsvTable control && newValue is CsvTableViewMode)
		{
			control.SetupViewModelEvents();
		}
	}

	/// <summary>
	/// 覆盖OnApplyTemplate方法以获取控件引用
	/// </summary>
	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		
		// 获取搜索输入框引用
		_searchEntry = GetTemplateChild("PART_SearchEntry") as Entry;
		
		// 绑定搜索文本框事件
		if (_searchEntry != null)
		{
			_searchEntry.TextChanged += OnSearchTextChanged;
		}
		
		// 延迟执行同步列宽，确保布局已完成
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			SynchronizeColumnWidths();
		});
	}

	/// <summary>
	/// 构造函数 - 移除对_searchEntry的直接引用
	/// </summary>
	public CsvTable()
	{
		InitializeComponent();
		ViewModel = new CsvTableViewMode();
		
		// 初始化滚动相关组件
		_scrollTimer = Application.Current.Dispatcher.CreateTimer();
		_scrollTimer.Interval = TimeSpan.FromMilliseconds(16);
		_scrollTimer.Tick += OnScrollTimerTick;
		
		_lastDragTime = DateTime.Now;
		_lastValidVelocity = 0;
		_minVelocityThreshold = 0.5;
		_scrollAcceleration = 0.92;
		
		// 绑定触摸事件
		tableGraphicsView.StartInteraction += OnStartInteraction;
		tableGraphicsView.DragInteraction += OnDragInteraction;
		tableGraphicsView.EndInteraction += OnEndInteraction;
		
		// 初始化数据源监测
		InitializeDataSourceWatcher();
		
		// 其他初始化代码...
	}

	/// <summary>
	/// 确保表头和表格内容使用相同的列宽
	/// </summary>
	private void SynchronizeColumnWidths()
	{
		if (Width <= 0 || ViewModel == null) return;
		
		// 计算可用宽度，考虑边距
		double availableWidth = Width - 20; // 减去Grid的左右Padding
		
		// 重新计算列宽
		ViewModel.RecalculateColumnWidths(availableWidth);
		
		// 同步表格内容的单元格宽度
		if (ViewModel.FilteredRows != null && ViewModel.FilteredRows.Count > 0)
		{
			// 遍历每一行
			foreach (var row in ViewModel.FilteredRows)
			{
				// 确保行中的每个单元格与表头定义的列宽一致
				for (int i = 0; i < row.Cells.Count && i < ViewModel.ColumnDefinitions.Count; i++)
				{
					// 设置单元格宽度与表头列宽一致
					row.Cells[i].Width = ViewModel.ColumnDefinitions[i].Width;
				}
			}
		}
		
		// 强制重绘
		tableGraphicsView?.Invalidate();
	}

	/// <summary>
	/// 大小变更处理
	/// </summary>
	private void OnSizeChanged(object sender, EventArgs e)
	{
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
		{
			if (Width > 0 && Height > 0)
			{
				// 同步列宽
				SynchronizeColumnWidths();

				// 更新GraphicsView的视口高度
				if (tableGraphicsView != null)
				{
					TableDrawable.ViewportHeight = tableGraphicsView.Height;
				}

				// 如果有待处理的数据源，加载它
				if (_pendingDataSource != null)
				{
					ViewModel.SetBindingData(_pendingDataSource, Width - 20);
					TableDrawable.Rows = ViewModel.FilteredRows;
					_pendingDataSource = null;
					
					// 同步新加载数据的列宽
					SynchronizeColumnWidths();
				}
				
				// 强制重绘
				tableGraphicsView?.Invalidate();
			}
		});
	}

	/// <summary>
	/// 控件处理程序变更事件
	/// </summary>
	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		
		if (tableGraphicsView != null && tableGraphicsView.Handler != null)
		{
			Dispatcher.Dispatch(() => 
			{
				// 设置视口高度
				TableDrawable.ViewportHeight = tableGraphicsView.Height;
				
				#if WINDOWS
				// Windows平台鼠标滚轮处理
				SetupWindowsWheelScrolling();
				#elif ANDROID
				// Android平台特定处理
				SetupAndroidView();
				#endif
				
				// 刷新视图
				tableGraphicsView.Invalidate();
			});
		}
	}

	#region 属性变更处理
	/// <summary>
	/// 搜索框可见性变更处理
	/// </summary>
	private static void OnIsSearchVisibleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CsvTable control)
		{
			control.OnPropertyChanged(nameof(TableGridRow));
			control.OnPropertyChanged(nameof(TableGridRowSpan));
		}
	}

	/// <summary>
	/// 表格可编辑状态变更处理
	/// </summary>
	private static void OnIsEditableChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CsvTable control)
		{
			control.ViewModel.IsEditable = (bool)newValue;
		}
	}

	/// <summary>
	/// 数据源变更处理
	/// </summary>
	private static void OnDataSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var control = (CsvTable)bindable;
		
		if (newValue != null)
		{
			var data = (CsvTableDataMode)newValue;
			
			// 延迟一帧执行，确保属性已更新
			control.Dispatcher.Dispatch(() => {
				// 确保ViewModel已初始化
				if (control.ViewModel == null)
				{
					control.ViewModel = new CsvTableViewMode();
					control.SetupViewModelEvents();
				}
				
				// 明确重置滚动位置，因为数据源变更是重大变化
				control._scrollY = 0;
				
				// 设置数据源到ViewModel
				control.ViewModel.SetBindingData(data, control.Width > 0 ? control.Width - 20 : 800);
				
				// 更新表格数据
				control.TableDrawable.Rows = control.ViewModel.FilteredRows;
				control.TableDrawable.ScrollY = control._scrollY;
				
				// 更新空视图状态
				control.OnPropertyChanged(nameof(control.IsEmptyViewVisible));
				
				// 强制重绘
				control.tableGraphicsView?.Invalidate();
			});
		}
	}

	/// <summary>
	/// 文件路径列表变更处理
	/// </summary>
	private static void OnFilePathsChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CsvTable control && newValue is List<string> filePaths)
		{
			control.UpdateFileNames(filePaths);
		}
	}

	/// <summary>
	/// 文件选择器可见性变更处理
	/// </summary>
	private static void OnIsPickerVisibleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CsvTable control)
		{
			control.OnPropertyChanged(nameof(TableGridRow));
			control.OnPropertyChanged(nameof(TableGridRowSpan));
		}
	}

	/// <summary>
	/// 选中文件路径变更处理
	/// </summary>
	private static void OnSelectedFilePathChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CsvTable control && newValue is string filePath)
		{
			control.FileSelected?.Invoke(filePath);
		}
	}
	#endregion

	/// <summary>
	/// 文本框内容变更处理
	/// </summary>
	private void Entry_TextChanged(object sender, TextChangedEventArgs e)
	{
		ViewModel.HasChanges = true;
	}

	#region 文件选择相关方法
	/// <summary>
	/// 更新文件名列表
	/// </summary>
	/// <param name="filePaths">文件路径列表</param>
	private void UpdateFileNames(List<string> filePaths)
	{
		if (filePaths == null || filePaths.Count == 0)
		{
			FileNames.Clear();
			_filePathDict.Clear();
			return;
		}

		FileNames.Clear();
		_filePathDict.Clear();

		foreach (var path in filePaths)
		{
			if (File.Exists(path))
			{
				string fileName = Path.GetFileName(path);
				FileNames.Add(fileName);
				_filePathDict[fileName] = path;
			}
		}

		// 如果有文件，默认选择第一个
		if (FileNames.Count > 0 && string.IsNullOrEmpty(SelectedFilePath))
        {
			_picker.SelectedIndex = 0;
            IsPickerVisible = FileNames.Count > 1;
        }
	}

	/// <summary>
	/// 文件选择器选择项变更处理
	/// </summary>
	public void OnPickerSelectedIndexChanged(object sender, EventArgs e)
	{
		if (sender is Picker picker && picker.SelectedIndex != -1)
		{
			string fileName = picker.Items[picker.SelectedIndex];
			if (_filePathDict.TryGetValue(fileName, out string filePath))
			{
				SelectedFilePath = filePath;
			}
		}
	}
	#endregion

	/// <summary>
	/// 处理ViewModel.FilteredRows变化
	/// </summary>
	private void OnFilteredRowsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		TableDrawable.Rows = ViewModel.FilteredRows;
		OnPropertyChanged(nameof(IsEmptyViewVisible));
		tableGraphicsView.Invalidate();
	}

	/// <summary>
	/// 强制重新加载数据
	/// </summary>
	public void ForceReloadData()
	{
		if (DataSource != null)
		{
			// 确保一次性加载所有数据
			Dispatcher.Dispatch(() => {
				// 明确重置滚动位置，因为这是完全重新加载数据
				_scrollY = 0;
				
				// 重新设置数据源，加载全部数据
				ViewModel.SetBindingData(DataSource, Width > 0 ? Width - 20 : 800);
				TableDrawable.Rows = ViewModel.FilteredRows;
				TableDrawable.ScrollY = _scrollY;
				
				// 更新空视图状态
				OnPropertyChanged(nameof(IsEmptyViewVisible));
				
				// 强制重绘
				tableGraphicsView?.Invalidate();
			});
		}
	}

	/// <summary>
	/// 设置ViewModel属性变更事件处理
	/// </summary>
	private void SetupViewModelEvents()
	{
		if (ViewModel != null)
		{
			// 监听FilteredRows的变化
			ViewModel.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ViewModel.FilteredRows))
				{
					// 搜索结果更新后立即更新视图
					Dispatcher.Dispatch(() =>
					{
						// 保存当前滚动位置
						double currentScrollY = _scrollY;
						
						// 只在下列情况重置滚动位置：
						// 1. 从无搜索到有搜索 - 开始新搜索时
						bool isNewSearch = string.IsNullOrEmpty(ViewModel.LastSearchText) && 
										 !string.IsNullOrEmpty(ViewModel.SearchText);
						
						if (isNewSearch)
						{
							// 新搜索时重置滚动位置
							currentScrollY = 0;
						}
						
						// 更新表格数据
						TableDrawable.Rows = ViewModel.FilteredRows;
						
						// 确保滚动位置不超过内容高度
						double contentHeight = TableDrawable.Rows.Count * TableDrawable.RowHeight;
						double maxScrollY = Math.Max(0, contentHeight - TableDrawable.ViewportHeight);
						
						// 应用当前滚动位置，确保在有效范围内
						_scrollY = Math.Min(currentScrollY, maxScrollY);
						TableDrawable.ScrollY = _scrollY;
						
						// 更新空视图状态
						OnPropertyChanged(nameof(IsEmptyViewVisible));
						
						// 强制立即重绘
						tableGraphicsView?.Invalidate();
					});
				}
			};
		}
	}

	/// <summary>
	/// 计算内容总高度
	/// </summary>
	/// <returns>表格内容的总高度（像素）</returns>
	private double CalculateContentHeight()
	{
		if (TableDrawable.Rows == null || TableDrawable.Rows.Count == 0)
			return 0;
			
		return TableDrawable.Rows.Count * TableDrawable.RowHeight;
	}

	/// <summary>
	/// 搜索文本变更处理
	/// </summary>
	private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
	{
		// 执行搜索
		ViewModel?.SearchCommand?.Execute(null);
	}

	#region 滚动交互处理
	/// <summary>
	/// 开始触摸交互处理
	/// </summary>
	private void OnStartInteraction(object sender, TouchEventArgs e)
	{
		if (e.Touches.Length == 0) return;
		
		_isDragging = true;
		_lastTouchY = e.Touches[0].Y;
		_scrollVelocity = 0;
		_scrollTimer.Stop();
		_lastDragTime = DateTime.Now;
	}

	/// <summary>
	/// 触摸拖动处理
	/// </summary>
	private void OnDragInteraction(object sender, TouchEventArgs e)
	{
		if (!_isDragging || e.Touches.Length == 0) return;
		
		double currentY = e.Touches[0].Y;
		double deltaY = currentY - _lastTouchY;
		
		// 记录时间
		DateTime now = DateTime.Now;
		TimeSpan elapsed = now - _lastDragTime;
		double milliseconds = Math.Max(1, elapsed.TotalMilliseconds);
		
		// 更新滚动位置
		if (Math.Abs(deltaY) > 0.1) // 只处理实际的移动
		{
			UpdateScrollPosition(deltaY);
			
			// 只有实际移动时才更新速度和最后有效速度
			_scrollVelocity = deltaY;
			
			// 记录有效的拖动速度
			if (Math.Abs(deltaY) > 2) // 忽略微小移动
			{
				_lastValidVelocity = deltaY;
			}
		}
		
		_lastTouchY = currentY;
		_lastDragTime = now;
	}

	/// <summary>
	/// 结束触摸交互处理，启动惯性滚动
	/// </summary>
	private void OnEndInteraction(object sender, TouchEventArgs e)
	{
		if (!_isDragging) return;
		
		_isDragging = false;
		
		// 使用最后一个有效的拖动速度，而不是可能被重置的当前速度
		double velocityToUse = Math.Abs(_lastValidVelocity) > Math.Abs(_scrollVelocity) ? 
							  _lastValidVelocity : _scrollVelocity;
		
		// 确保速度值合理
		velocityToUse = Math.Clamp(velocityToUse, -200, 200);
		
		// 如果有足够的速度，启动惯性滚动
		if (Math.Abs(velocityToUse) > _minVelocityThreshold)
		{
			// 保存到实例变量以供计时器使用
			_scrollVelocity = velocityToUse;
			
			// 重置计时器并启动
			_scrollTimer.Stop();
			_scrollTimer.Start();
		}
		
		// 重置最后有效速度
		_lastValidVelocity = 0;
	}

	/// <summary>
	/// 滚动计时器回调，处理惯性滚动
	/// </summary>
	private void OnScrollTimerTick(object sender, EventArgs e)
	{
		// 应用当前速度滚动
		UpdateScrollPosition(_scrollVelocity);
		
		// 应用减速
		_scrollVelocity *= _scrollAcceleration;
		
		// 当速度小于阈值时停止滚动
		if (Math.Abs(_scrollVelocity) < _minVelocityThreshold)
		{
			_scrollTimer.Stop();
		}
	}
	#endregion

	#region 滚动位置计算与更新
	/// <summary>
	/// 统一的滚动位置更新方法
	/// </summary>
	/// <param name="delta">滚动位置变化量</param>
	private void UpdateScrollPosition(double delta)
	{
		// 更新滚动位置 (注意：向下滚动时delta为正，与ScrollGraphicsView保持一致)
		_scrollY -= delta;
		
		// 限制滚动范围
		_scrollY = Math.Max(0, _scrollY);
		
		// 计算内容总高度和最大滚动位置
		double contentHeight = CalculateContentHeight();
		double maxScrollY = Math.Max(0, contentHeight - TableDrawable.ViewportHeight);
		_scrollY = Math.Min(_scrollY, maxScrollY);
		
		// 更新视图模型的滚动位置
		TableDrawable.ScrollY = _scrollY;
		
		// 刷新绘图
		tableGraphicsView?.Invalidate();
	}
	#endregion

	#region 平台特定处理
	#if WINDOWS
	/// <summary>
	/// Windows平台滚轮处理
	/// </summary>
	private void SetupWindowsWheelScrolling()
	{
		var nativeView = tableGraphicsView.Handler.PlatformView as Microsoft.UI.Xaml.FrameworkElement;
		if (nativeView != null)
		{
			nativeView.PointerWheelChanged += (s, e) => 
			{
				var delta = e.GetCurrentPoint(nativeView).Properties.MouseWheelDelta;
				// 将滚轮增量转换为适当的滚动距离
				UpdateScrollPosition(delta / _wheelScrollFactor);
				e.Handled = true;
			};
		}
	}
	#elif ANDROID
	/// <summary>
	/// Android平台特定设置
	/// </summary>
	private void SetupAndroidView()
	{
		if (tableGraphicsView.Handler.PlatformView is Android.Views.View androidView)
		{
			// 确保正确裁剪和优化Android视图
			androidView.ClipToOutline = true;
		}
	}
	#endif
	#endregion

	/// <summary>
	/// 初始化数据源监测机制
	/// </summary>
	private void InitializeDataSourceWatcher()
	{
		// 创建定时器
		_dataSourceWatchTimer = Application.Current.Dispatcher.CreateTimer();
		_dataSourceWatchTimer.Interval = TimeSpan.FromMilliseconds(DATA_SOURCE_WATCH_INTERVAL);
		_dataSourceWatchTimer.Tick += OnDataSourceWatchTick;
		_dataSourceWatchTimer.Start();
	}

	/// <summary>
	/// 定时检查数据源是否有变化
	/// </summary>
	private void OnDataSourceWatchTick(object sender, EventArgs e)
	{
		if (DataSource == null) return;
		
		// 计算当前数据源哈希值
		int currentHash = CalculateDataSourceHash();
		
		// 如果哈希值变化，表示数据源内容发生变化
		if (currentHash != _lastDataSourceHash)
		{
			// 记录新哈希值
			_lastDataSourceHash = currentHash;
			
			// 更新UI，但保持滚动位置
			UpdateUIWithCurrentScrollPosition();
		}
	}

	/// <summary>
	/// 计算数据源的哈希值，用于检测变化
	/// </summary>
	private int CalculateDataSourceHash()
	{
		if (DataSource == null) return 0;
		
		int hash = 17;
		
		// 计算表头哈希值
		if (DataSource.Headers != null)
		{
			foreach (var header in DataSource.Headers)
			{
				hash = hash * 31 + (header?.GetHashCode() ?? 0);
			}
		}
		
		// 计算行数据哈希值（取样前100行以提高性能）
		if (DataSource.Rows != null)
		{
			int sampleSize = Math.Min(100, DataSource.Rows.Count);
			for (int i = 0; i < sampleSize; i++)
			{
				var row = DataSource.Rows[i];
				if (row != null)
				{
					foreach (var cell in row)
					{
						hash = hash * 31 + (cell?.GetHashCode() ?? 0);
					}
				}
			}
			
			// 添加行总数到哈希值
			hash = hash * 31 + DataSource.Rows.Count;
		}
		
		return hash;
	}

	/// <summary>
	/// 使用当前滚动位置更新UI
	/// </summary>
	private void UpdateUIWithCurrentScrollPosition()
	{
		// 保存当前滚动位置
		double currentScrollY = _scrollY;
		
		// 更新数据源到ViewModel
		ViewModel.SetBindingData(DataSource, Width > 0 ? Width - 20 : 800);
		
		// 更新表格数据
		TableDrawable.Rows = ViewModel.FilteredRows;
		
		// 确保滚动位置不超过内容高度
		double contentHeight = TableDrawable.Rows.Count * TableDrawable.RowHeight;
		double maxScrollY = Math.Max(0, contentHeight - TableDrawable.ViewportHeight);
		
		// 应用当前滚动位置，确保在有效范围内
		_scrollY = Math.Min(currentScrollY, maxScrollY);
		TableDrawable.ScrollY = _scrollY;
		
		// 更新空视图状态
		OnPropertyChanged(nameof(IsEmptyViewVisible));
		
		// 强制重绘
		tableGraphicsView?.Invalidate();
	}

	/// <summary>
	/// 资源清理
	/// </summary>
	public void Dispose()
	{
		// 停止并清理计时器
		_scrollTimer?.Stop();
		_dataSourceWatchTimer?.Stop();
		
		// 移除事件处理
		if (tableGraphicsView != null)
		{
			tableGraphicsView.StartInteraction -= OnStartInteraction;
			tableGraphicsView.DragInteraction -= OnDragInteraction;
			tableGraphicsView.EndInteraction -= OnEndInteraction;
		}
		
		// 允许GC回收
		_scrollTimer = null;
		_dataSourceWatchTimer = null;
		
		GC.SuppressFinalize(this);
	}

	// 析构函数
	~CsvTable()
	{
		Dispose();
	}
}
