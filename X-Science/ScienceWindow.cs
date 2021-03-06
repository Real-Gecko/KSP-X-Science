﻿using System;
using UnityEngine;
using KSP;



namespace ScienceChecklist {


	// Checklist window where users can view all possible science and filter it
	internal sealed class ScienceWindow {
		#region FIELDS
		private	ScienceChecklistAddon	_parent;
		private SettingsWindow			_settingsWindow;
		private	HelpWindow				_helpWindow;

		// Public stuff for visibility
		public event EventHandler OnCloseEvent;
		public event EventHandler OnOpenEvent;
		public bool IsVisible { get; set; }

		private Rect _rect;
		private Rect _rect3;
		private Vector2 _scrollPos;
		private Vector2 _compactScrollPos;
		private GUIStyle _labelStyle;
		private GUIStyle _horizontalScrollbarOnboardStyle;
		private GUIStyle _progressLabelStyle;
		private GUIStyle _situationStyle;
		private GUIStyle _experimentProgressLabelStyle;
		private GUIStyle _tooltipStyle;
		private GUIStyle _tooltipBoxStyle;
		private GUIStyle _compactWindowStyle;
		private GUIStyle _compactLabelStyle;
		private GUIStyle _compactSituationStyle;
		private GUIStyle _compactButtonStyle;
		private GUIStyle _closeButtonStyle;
		private GUISkin _skin;

		private string _lastTooltip;
		private bool _compactMode;

		private readonly Texture2D _progressTexture;
		private readonly Texture2D _completeTexture;
		private readonly Texture2D _progressTextureCompact;
		private readonly Texture2D _completeTextureCompact;
		private readonly Texture2D _emptyTexture;
		private readonly Texture2D _currentSituationTexture;
		private readonly Texture2D _currentVesselTexture;
		private readonly Texture2D _unlockedTexture;
		private readonly Texture2D _allTexture;
		private readonly Texture2D _searchTexture;
		private readonly Texture2D _clearSearchTexture;
		private readonly Texture2D _minimizeTexture;
		private readonly Texture2D _maximizeTexture;
		private readonly Texture2D _closeTexture;
		private readonly Texture2D _helpTexture;
		private readonly Texture2D _settingsTexture;

		private readonly ExperimentFilter _filter;
		private readonly Logger _logger;
		private readonly int _windowId = UnityEngine.Random.Range( 0, int.MaxValue );
		private readonly int _window2Id = UnityEngine.Random.Range( 0, int.MaxValue );
		private readonly int _window3Id = UnityEngine.Random.Range( 0, int.MaxValue );
		#endregion



		#region Constructor
		public ScienceWindow ( ScienceChecklistAddon Parent, SettingsWindow settingsWindow, HelpWindow helpWindow )
		{
			_parent = Parent;
			_settingsWindow = settingsWindow;
			_helpWindow = helpWindow;

			_logger = new Logger(this);
			_rect = new Rect(40, 40, 500, 400);
			_rect3 = new Rect(40, 40, 400, 200);
			_scrollPos = new Vector2();
			_filter = new ExperimentFilter( _parent );

			_progressTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.scienceProgress.png", 13, 13 );
			_completeTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.scienceComplete.png", 13, 13 );
			_progressTextureCompact =				TextureHelper.FromResource( "ScienceChecklist.icons.scienceProgressCompact.png", 8, 8 );
			_completeTextureCompact =				TextureHelper.FromResource( "ScienceChecklist.icons.scienceCompleteCompact.png", 8, 8 );
			_currentSituationTexture =				TextureHelper.FromResource( "ScienceChecklist.icons.currentSituation.png", 25, 21 );
			_currentVesselTexture =					TextureHelper.FromResource( "ScienceChecklist.icons.currentVessel.png", 25, 21 );
			_unlockedTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.unlocked.png", 25, 21 );
			_allTexture =							TextureHelper.FromResource( "ScienceChecklist.icons.all.png", 25, 21 );
			_searchTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.search.png", 25, 21 );
			_clearSearchTexture =					TextureHelper.FromResource( "ScienceChecklist.icons.clearSearch.png", 25, 21 );
			_minimizeTexture=						TextureHelper.FromResource( "ScienceChecklist.icons.minimize.png", 16, 16 );
			_maximizeTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.maximize.png", 16, 16 );
			_closeTexture =							TextureHelper.FromResource( "ScienceChecklist.icons.close.png", 16, 16 );
			_helpTexture =							TextureHelper.FromResource( "ScienceChecklist.icons.help.png", 16, 16 );
			_settingsTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.settings.png", 16, 16 );

			_emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			_emptyTexture.SetPixels(new[] { Color.clear });
			_emptyTexture.Apply();

			_parent.Config.HideCompleteEventsChanged += ( s, e ) => RefreshFilter( s, e );
			_parent.Config.CompleteWithoutRecoveryChanged += ( s, e ) => RefreshFilter( s, e );

			_parent.ScienceEventHandler.FilterUpdateEvent += ( s, e ) => RefreshFilter( s, e );
			_parent.ScienceEventHandler.SituationChanged += ( s, e ) => UpdateSituation( s, e );

		}
		#endregion



		#region Events called when science changes
		// Refreshes the experiment filter.
		// This is the lightest update used when the vessel changes
		public void RefreshFilter( object sender, EventArgs e )
		{
			//			_logger.Trace("RefreshFilter");
			_filter.UpdateFilter( );
		}

		public void UpdateSituation( object sender, NewSituationData e )
		{
			//			_logger.Trace("UpdateSituation");


// Bung new situation into filter and recalculate everything
			_filter.CurrentSituation = new Situation( e._body, e._situation, e._biome, e._subBiome );
			_filter.UpdateFilter( );
		}


		#endregion



		#region METHODS (PUBLIC)



		public WindowSettings BuildSettings( )
		{
			WindowSettings W = new WindowSettings( );
			W.Name = "ScienceCheckList";
			W.Top = (int)_rect.yMin;
			W.Left = (int)_rect.xMin;
			W.CompactTop = (int)_rect3.yMin;
			W.CompactLeft = (int)_rect3.xMin;
			W.Visible = IsVisible;
			W.Compacted = _compactMode;
			W.FilterMode = _filter.DisplayMode;
			W.FilterText = _filter.Text;

			return W;
		}



		public void ApplySettings( WindowSettings W )
		{
			_rect.yMin = W.Top;
			_rect.xMin = W.Left;
			_rect.yMax = W.Top + 400;
			_rect.xMax = W.Left + 500;

			_rect3.yMin = W.CompactTop;
			_rect3.xMin = W.CompactLeft;
			_rect3.yMax = W.CompactTop + 200;
			_rect3.xMax = W.CompactLeft + 400;

			_compactMode = W.Compacted;
			_filter.DisplayMode = W.FilterMode;
			_filter.Text = W.FilterText;
			_filter.UpdateFilter( );

			if( W.Visible )
				OnOpenEvent( this, EventArgs.Empty );
			else
				OnCloseEvent( this, EventArgs.Empty );
		}



		/// <summary>
		/// Draws the window if it is visible.
		/// </summary>
		public void Draw( )
		{
			if( !IsVisible )
			{
				return;
			}
			if( !GameHelper.AllowWindow( ) )
			{
				IsVisible = false;
				OnCloseEvent( this, EventArgs.Empty );
			}

			if( _skin == null )
			{
				// Initialize our skin and styles.
				_skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

				_skin.horizontalScrollbarThumb.fixedHeight = 13;
				_skin.horizontalScrollbar.fixedHeight = 13;

				_labelStyle = new GUIStyle(_skin.label) {
					fontSize = 11,
					fontStyle = FontStyle.Italic,
				};

				_progressLabelStyle = new GUIStyle(_skin.label) {
					fontStyle = FontStyle.BoldAndItalic,
					alignment = TextAnchor.MiddleCenter,
					fontSize = 11,
					normal = {
						textColor = new Color(0.322f, 0.298f, 0.004f),
					},
				};

				_situationStyle = new GUIStyle(_progressLabelStyle) {
					fontSize = 13,
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Normal,
					fixedHeight = 25,
					contentOffset = new Vector2(0, 6),
					wordWrap = true,
					normal = {
						textColor = new Color(0.7f, 0.8f, 0.8f),
					}
				};

				_experimentProgressLabelStyle = new GUIStyle(_skin.label) {
					padding = new RectOffset(0, 0, 4, 0),
				};

				_horizontalScrollbarOnboardStyle = new GUIStyle(_skin.horizontalScrollbar) {
					normal = {
						background = _emptyTexture,
					},
				};

				_compactWindowStyle = new GUIStyle(_skin.window) {
					padding = new RectOffset(0, 4, 4, 4),
				};

				_compactLabelStyle = new GUIStyle(_labelStyle) {
					fontSize = 9,
				};

				_compactSituationStyle = new GUIStyle(_situationStyle) {
					fontSize = 11,
					contentOffset = new Vector2( 0, 3 ),
				};

				_compactButtonStyle = new GUIStyle(_skin.button) {
					padding = new RectOffset(),
					fixedHeight = 16
				};
				_closeButtonStyle = new GUIStyle( _skin.button )
				{
					// int left, int right, int top, int bottom
					padding = new RectOffset(2, 2, 2, 2),
					margin = new RectOffset(1, 1, 1, 1),
					stretchWidth = false,
					stretchHeight = false,
					alignment = TextAnchor.MiddleCenter,
				};
			}

			var oldSkin = GUI.skin;
			GUI.skin = _skin;

			if( _compactMode )
			{
				_rect3 = GUILayout.Window(_window3Id, _rect3, DrawCompactControls, string.Empty, _compactWindowStyle);
			}
			else
			{
				_rect = GUILayout.Window( _windowId, _rect, DrawControls, "[x] Science!");
			}



			if (!string.IsNullOrEmpty(_lastTooltip)) {
				_tooltipStyle = _tooltipStyle ?? new GUIStyle(_skin.window) {
					normal = {
						background = GUI.skin.window.normal.background
					},
					wordWrap = true
				};

				_tooltipBoxStyle = _tooltipBoxStyle ?? new GUIStyle( _skin.box )
				{
					// int left, int right, int top, int bottom
					padding = new RectOffset( 4, 4, 4, 4 ),
					wordWrap = true
				};

				float boxHeight = _tooltipBoxStyle.CalcHeight( new GUIContent( _lastTooltip ), 190 );
				GUI.Window( _window2Id, new Rect( Mouse.screenPos.x + 15, Mouse.screenPos.y + 15, 200, boxHeight + 10 ), x =>
				{
					GUI.Box( new Rect( 5, 5, 190, boxHeight ), _lastTooltip, _tooltipBoxStyle );
				}, string.Empty, _tooltipStyle );
			}

			GUI.skin = oldSkin;
		}




		#endregion



		#region METHODS (PRIVATE)

		/// <summary>
		/// Draws the controls inside the window.
		/// </summary>
		/// <param name="windowId"></param>
		private void DrawControls (int windowId)
		{
			DrawTitleBarButtons( _rect );


			GUILayout.BeginHorizontal( );

			GUILayout.BeginVertical(GUILayout.Width(480), GUILayout.ExpandHeight(true));

			ProgressBar(
				new Rect (10, 27, 480, 13),
				_filter.TotalCount == 0 ? 1 : _filter.CompleteCount,
				_filter.TotalCount == 0 ? 1 : _filter.TotalCount,
				0,
				false,
				false);

			GUILayout.Space(20);

			GUILayout.BeginHorizontal( );
			GUILayout.Label
			(
				new GUIContent(
					string.Format("{0}/{1} complete.", _filter.CompleteCount, _filter.TotalCount),
					string.Format( "{0} remaining\n{1:0.#} mits", _filter.TotalCount - _filter.CompleteCount, _filter.TotalScience - _filter.CompletedScience )
				),
				_experimentProgressLabelStyle,
				GUILayout.Width( 150 )
			);
			GUILayout.FlexibleSpace();
			GUILayout.Label(new GUIContent(_searchTexture));
			_filter.Text = GUILayout.TextField(_filter.Text, GUILayout.Width(150));

			if (GUILayout.Button(new GUIContent(_clearSearchTexture, "Clear search"), GUILayout.Width(25), GUILayout.Height(23))) {
				_filter.Text = string.Empty;
			}

			GUILayout.EndHorizontal();

			_scrollPos = GUILayout.BeginScrollView(_scrollPos, _skin.scrollView);
			var i = 0;
			if( _filter.DisplayScienceInstances == null )
				_logger.Trace( "DisplayExperiments is null" );
			else
			{
				for( ; i < _filter.DisplayScienceInstances.Count; i++ )
				{
					var rect = new Rect( 5, 20 * i, _filter.DisplayScienceInstances.Count > 13 ? 490 : 500, 20 );
				if (rect.yMax < _scrollPos.y || rect.yMin > _scrollPos.y + 400) {
					continue;
				}

				var experiment = _filter.DisplayScienceInstances[ i ];
				DrawExperiment( experiment, rect, false, _labelStyle );
			}
			}

			GUILayout.Space(20 * i);
			GUILayout.EndScrollView();
			
			GUILayout.BeginHorizontal();


			var TextWidth = 290;
			var NumButtons = 3;
			GUIContent[ ] FilterButtons = {
					new GUIContent(_currentSituationTexture, "Show experiments available right now"),
					new GUIContent(_currentVesselTexture, "Show experiments available on this vessel"),
					new GUIContent(_unlockedTexture, "Show all unlocked experiments")
				};
			if( _parent.Config.AllFilter )
			{
				Array.Resize( ref FilterButtons, 4 );
				FilterButtons[ 3 ] = new GUIContent(_allTexture, "Show all experiments");
				TextWidth = 260;
				NumButtons = 4;
			}
			else
			{
				if( _filter.DisplayMode == DisplayMode.All )
				{
					_filter.DisplayMode = DisplayMode.Unlocked;
					_filter.UpdateFilter( );
				}
			}

			_filter.DisplayMode = (DisplayMode)GUILayout.SelectionGrid( (int)_filter.DisplayMode, FilterButtons, NumButtons );

			GUILayout.FlexibleSpace();

			if (_filter.CurrentSituation != null) {
				var desc = _filter.CurrentSituation.Description;
				GUILayout.Box( char.ToUpper( desc[ 0 ] ) + desc.Substring( 1 ), _situationStyle, GUILayout.Width( TextWidth ) );
			}
			GUILayout.FlexibleSpace( );
		
			GUILayout.EndHorizontal();
			GUILayout.EndVertical ();

			GUILayout.EndHorizontal ();
			GUI.DragWindow();

			if (Event.current.type == EventType.Repaint && GUI.tooltip != _lastTooltip) {
				_lastTooltip = GUI.tooltip;
			}
			
			// If this window gets focus, it pushes the tooltip behind the window, which looks weird.
			// Just hide the tooltip while mouse buttons are held down to avoid this.
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
				_lastTooltip = string.Empty;
			}
		}



		/// <summary>
		/// Draws the controls for the window in compact mode.
		/// </summary>
		/// <param name="windowId"></param>
		private void DrawCompactControls( int windowId )
		{
			GUILayout.BeginHorizontal( );
			GUILayout.Label( "", GUILayout.Height( 20 ) );
			GUILayout.EndHorizontal( );

			GUILayout.BeginVertical();
			_compactScrollPos = GUILayout.BeginScrollView(_compactScrollPos);
			var i = 0;
			if( _filter.DisplayScienceInstances != null )
			{
				for( ; i < _filter.DisplayScienceInstances.Count; i++ )
				{

					var rect = new Rect( 5, 15 * i, _filter.DisplayScienceInstances.Count > 11 ? 405 : 420, 20 );
					if (rect.yMax < _compactScrollPos.y || rect.yMin > _compactScrollPos.y + 400) {
						continue;
					}

					var experiment = _filter.DisplayScienceInstances[ i ];
					DrawExperiment(experiment, rect, true, _compactLabelStyle);
				}
			}
			else
				_logger.Trace( "DisplayExperiments is null" );
			GUILayout.Space(15 * i);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();



			if( _filter.CurrentSituation != null )
			{
				var desc = _filter.CurrentSituation.Description;
				GUI.Box( new Rect( 28, 0, _rect3.width - 100, 16 ), char.ToUpper( desc[ 0 ] ) + desc.Substring( 1 ), _compactSituationStyle );
			}
			DrawTitleBarButtons( _rect3, true );

			

			GUI.DragWindow();
			if( Event.current.type == EventType.Repaint && GUI.tooltip != _lastTooltip )
			{
				_lastTooltip = GUI.tooltip;
			}

			// If this window gets focus, it pushes the tooltip behind the window, which looks weird.
			// Just hide the tooltip while mouse buttons are held down to avoid this.
			if( Input.GetMouseButton( 0 ) || Input.GetMouseButton( 1 ) || Input.GetMouseButton( 2 ) )
			{
				_lastTooltip = string.Empty;
			}
		}


		private void DrawTitleBarButtons( Rect rect, bool NeedMaxIcon = false )
		{
			var closeContent = ( _closeTexture != null ) ? new GUIContent( _closeTexture, "Close window" ) : new GUIContent( "X", "Close window" );
			if( GUI.Button( new Rect( 4, 4, 20, 20 ), closeContent, _closeButtonStyle ) )
			{
				IsVisible = false;
				OnCloseEvent( this, EventArgs.Empty );
			}

			var helpContent = ( _helpTexture != null ) ? new GUIContent( _helpTexture, "Open help window" ) : new GUIContent( "?", "Open help window" );
			if( GUI.Button( new Rect( rect.width - 72, 4, 20, 20 ), helpContent, _closeButtonStyle ) )
			{
				_helpWindow.ToggleVisible( );
			}

			var setingsContent = ( _settingsTexture != null ) ? new GUIContent( _settingsTexture, "Open settings window" ) : new GUIContent( "S", "Open settings window" );
			if( GUI.Button( new Rect( rect.width - 48, 4, 20, 20 ), setingsContent, _closeButtonStyle ) )
			{
				_settingsWindow.ToggleVisible( );
			}

			GUIContent compactContent;
			if( NeedMaxIcon )
				compactContent = ( _maximizeTexture != null ) ? new GUIContent( _maximizeTexture, "Maximise window" ) : new GUIContent( "S", "Maximise window" );
			else
				compactContent = ( _minimizeTexture != null ) ? new GUIContent( _minimizeTexture, "Compact window" ) : new GUIContent( "S", "Compact window" );
			if( GUI.Button( new Rect( rect.width - 24, 4, 20, 20 ), compactContent, _closeButtonStyle ) )
			{
				_compactMode = !_compactMode;
			}
		}



		/// <summary>
		/// Draws an experiment inside the given Rect.
		/// </summary>
		/// <param name="exp">The experiment to render.</param>
		/// <param name="rect">The rect inside which the experiment should be rendered.</param>
		/// <param name="compact">Whether this experiment is compact.</param>
		/// <param name="labelStyle">The style to use for labels.</param>
		private void DrawExperiment (ScienceInstance exp, Rect rect, bool compact, GUIStyle labelStyle) {
			labelStyle.normal.textColor = exp.IsComplete ? Color.green : Color.yellow;
			var labelRect = new Rect(rect) {
				y = rect.y + (compact ? 1 : 3),
			};
			var progressRect = new Rect(rect) {
				xMin = rect.xMax - (compact ? 75 : 105),
				xMax = rect.xMax - (compact ? 40 : 40),
				y = rect.y + (compact ? 1 : 3),
			};
			GUI.Label(labelRect, exp.Description, labelStyle);
			GUI.skin.horizontalScrollbar.fixedHeight = compact ? 8 : 13;
			GUI.skin.horizontalScrollbarThumb.fixedHeight = compact ? 8 : 13;
			ProgressBar(progressRect, exp.CompletedScience, exp.TotalScience, exp.CompletedScience + exp.OnboardScience, !compact, compact);
		}

		/// <summary>
		/// Draws a progress bar inside the given Rect.
		/// </summary>
		/// <param name="rect">The rect inside which the progress bar should be rendered.</param>
		/// <param name="curr">The completed progress value.</param>
		/// <param name="total">The total progress value.</param>
		/// <param name="curr2">The shaded progress value (used to show onboard science).</param>
		/// <param name="showValues">Whether to draw the curr and total values on top of the progress bar.</param>
		/// <param name="compact">Whether this progress bar should be rendered in compact mode.</param>
		private void ProgressBar (Rect rect, float curr, float total, float curr2, bool showValues, bool compact) {
			var completeTexture = compact ? _completeTextureCompact : _completeTexture;
			var progressTexture = compact ? _progressTextureCompact : _progressTexture;
			var complete = curr > total || (total - curr < 0.1);
			if (complete) {
				curr = total;
			}
			var progressRect = new Rect(rect) {
				y = rect.y + (compact ? 3 : 1),
			};

			if (curr2 != 0 && !complete) {
				var complete2 = false;
				if (curr2 > total || (total - curr2 < 0.1)) {
					curr2 = total;
					complete2 = true;
				}
				_skin.horizontalScrollbarThumb.normal.background = curr2 < 0.1
					? _emptyTexture
					: complete2
						? completeTexture
						: progressTexture;

				GUI.HorizontalScrollbar(progressRect, 0, curr2 / total, 0, 1, _horizontalScrollbarOnboardStyle);
			}

			_skin.horizontalScrollbarThumb.normal.background = curr < 0.1
				? _emptyTexture
				: complete ? completeTexture : progressTexture;

			GUI.HorizontalScrollbar(progressRect, 0, curr / total, 0, 1);

			if (showValues) {
				var labelRect = new Rect(rect) {
					y = rect.y - 1,
				};
				GUI.Label(labelRect, string.Format("{0:0.#}  /  {1:0.#}", curr, total), _progressLabelStyle);
			}
		}



		#endregion
	}
}
