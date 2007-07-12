using System;
using System.Collections.Generic;
using Gtk;
using Cairo;

using Banshee.Data;

namespace Banshee.Data.Gui
{
    [Binding(Gdk.Key.A, Gdk.ModifierType.ControlMask, "SelectAll")]
    [Binding(Gdk.Key.A, Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask, "UnselectAll")]
    public class ListView<T> : Container
    {
        private static Gdk.Cursor resize_x_cursor = new Gdk.Cursor(Gdk.CursorType.SbHDoubleArrow);
    
        internal struct CachedColumn
        {
            public static readonly CachedColumn Zero;

            public Column Column;
            public int X1;
            public int X2;
            public int Width;
            public int ResizeX1;
            public int ResizeX2;
            public int Index;
        }
    
        private const int COLUMN_PADDING = 1;
        private const int BorderWidth = 6;
    
        private Gdk.Window bin_window;
        private Gdk.Window header_window;
        
        private ListViewGraphics graphics;
        private Cairo.Context cr;
        private Cairo.Context bin_cr;
        private Cairo.Context header_cr;
        
        private bool header_visible = true;
        
        private Adjustment vadjustment;
        private Adjustment hadjustment;
        private Gdk.Rectangle row_area;
        
        private int y_offset;
        private int x_offset;
        private int width;
        private int height;
        
        private int column_text_y;
        private int column_text_height;
        private Pango.Layout column_layout;
        private int resizing_column_index = -1;
        
        private IListModel<T> model;
        private ColumnController column_controller;
        private CachedColumn [] column_cache;
        
        private int focused_row_index = -1;
        private bool rules_hint = false;
        
        private Selection selection = new Selection();
        
        public Selection Selection {
            get { return selection; }
        }
        
        private int row_height = 0;
        protected int RowHeight {
            get {
                if(row_height == 0) {
                    int w_width;
                    Pango.Layout layout = new Pango.Layout(PangoContext);
                    layout.SetText("W");
                    layout.GetPixelSize(out w_width, out row_height);
                    row_height += 8;
                }
                
                return row_height;
            }
            
            set { row_height = value; }
        }
        
        private int header_height = 0;
        private int HeaderHeight {
            get {
                if(!header_visible) {
                    return 0;
                }
                
                if(header_height == 0) {
                    int w_width;
                    column_layout.SetText("W");
                    column_layout.GetPixelSize(out w_width, out column_text_height);
                    header_height = COLUMN_PADDING * 2 + column_text_height;
                    column_text_y = (header_height / 2) - (column_text_height / 2) - 1;
                }
                
                return header_height;
            }
        }
        
        public bool RulesHint {
            get { return rules_hint; }
            set { 
                rules_hint = value; 
                InvalidateBinWindow();
            }
        }
        
        public bool HeaderVisible {
            get { return header_visible; }
            set { 
                header_visible = value; 
                ShowHideHeader();
            }
        }
        
        private int RowsInView {
            get { return row_area.Height / RowHeight + 3; }
        }
        
        public ColumnController ColumnController {
            get { return column_controller; }
            set { 
                column_controller = value;
                RegenerateColumnCache();
                QueueDraw();
            }
        }
        
        public virtual IListModel<T> Model {
            get { return model; }
            set {
                if(model != value && model != null) {
                    model.Cleared -= OnModelCleared;
                    model.Reloaded -= OnModelReloaded;
                }
                
                model = value;
                
                if(model != null) {
                    model.Cleared += OnModelCleared;
                    model.Reloaded += OnModelReloaded;
                }
                
                RefreshViewForModel();
                
                Selection.Owner = this;
            }
        }
        
        public ListView()
        {
            column_layout = new Pango.Layout(PangoContext);
            CanFocus = true;
        }
        
#region Column Cache

     private void InvalidateColumnCache()
        {
            if(column_cache == null) {
                return;
            }
        
            for(int i = 0; i < column_cache.Length; i++) {
                column_cache[i].Column.VisibilityChanged -= OnColumnVisibilityChanged;
                column_cache[i] = CachedColumn.Zero;
            }
            
            column_cache = null;
        }
        
        private void RegenerateColumnCache()
        {
            InvalidateColumnCache();
            
            if(column_controller == null) {
                return;
            }
            
            int i = 0;
            column_cache = new CachedColumn[column_controller.Count];
            
            foreach(Column column in column_controller) {
                if(!column.Visible) {
                    continue;
                }
                
                column_cache[i] = new CachedColumn();
                column_cache[i].Column = column;
                column.VisibilityChanged += OnColumnVisibilityChanged;
                
                column_cache[i].Width = (int)Math.Round(((double)Allocation.Width * column.Width));
                column_cache[i].X1 = i == 0 ? 0 : column_cache[i - 1].X2;
                column_cache[i].X2 = column_cache[i].X1 + column_cache[i].Width;
                column_cache[i].ResizeX1 = column_cache[i].X1 + column_cache[i].Width - COLUMN_PADDING;
                column_cache[i].ResizeX2 = column_cache[i].ResizeX1 + 2;
                column_cache[i].Index = i;
                
                i++;
            }
            
            Array.Resize(ref column_cache, i);
        }
        
        private void OnColumnVisibilityChanged(object o, EventArgs args)
        {
            RegenerateColumnCache();
            QueueDraw();
        }

#endregion        
        
#region Gtk.Widget Overrides      

        protected override bool OnFocusInEvent(Gdk.EventFocus evnt)
        {
            return base.OnFocusInEvent(evnt);
        }
        
        protected override bool OnFocusOutEvent(Gdk.EventFocus evnt)
        {
            return base.OnFocusOutEvent(evnt);
        }
        
        protected override void OnMapped()
        {
            foreach(Widget child in Children) {
                if(child.Visible && !child.IsMapped) {
                    child.Map();
                }
            }
            
            bin_window.Show();
            
            if(header_visible) {
                header_window.Show();
            }
            
            GdkWindow.Show();
        }
        
        private void ShowHideHeader()
        {
            if(header_window == null) {
                return;
            }
            
            if(header_visible) {
                header_window.Show();
            } else {
                header_window.Hide();
            }
            
            MoveResizeWindows(Allocation);
        }
        
        private void MoveResizeWindows(Gdk.Rectangle allocation)
        {
             header_window.MoveResize(BorderWidth, BorderWidth, allocation.Width - (2 * BorderWidth), HeaderHeight);
             bin_window.MoveResize(BorderWidth, HeaderHeight + BorderWidth, allocation.Width - (2 * BorderWidth), 
                 allocation.Height - HeaderHeight - (2 * BorderWidth));
        }
        
        protected override void OnRealized()
        {
            WidgetFlags |= WidgetFlags.Realized;
            
            Gdk.WindowAttr attributes = new Gdk.WindowAttr();
            attributes.WindowType = Gdk.WindowType.Child;
            attributes.X = Allocation.X;
            attributes.Y = Allocation.Y;
            attributes.Width = Allocation.Width;
            attributes.Height = Allocation.Height;
            attributes.Visual = Visual;
            attributes.Wclass = Gdk.WindowClass.InputOutput;
            attributes.Colormap = Colormap;
            attributes.EventMask = (int)(
                Gdk.EventMask.VisibilityNotifyMask |
                Gdk.EventMask.ExposureMask);
            
            Gdk.WindowAttributesType attributes_mask = 
                Gdk.WindowAttributesType.X | 
                Gdk.WindowAttributesType.Y | 
                Gdk.WindowAttributesType.Visual | 
                Gdk.WindowAttributesType.Colormap;
                
            GdkWindow = new Gdk.Window(Parent.GdkWindow, attributes, attributes_mask);
            GdkWindow.UserData = Handle;
            
            // tree window
            
            attributes.X = 0;
            attributes.Y = HeaderHeight;
            attributes.Width = Allocation.Width;
            attributes.Height = Allocation.Height;
            attributes.EventMask = (int)(
                Gdk.EventMask.ExposureMask |
                Gdk.EventMask.ScrollMask |
                Gdk.EventMask.PointerMotionMask |
                Gdk.EventMask.EnterNotifyMask |
                Gdk.EventMask.LeaveNotifyMask |
                Gdk.EventMask.ButtonPressMask |
                Gdk.EventMask.ButtonReleaseMask |
                Events);
                
            bin_window = new Gdk.Window(GdkWindow, attributes, attributes_mask);
            bin_window.UserData = Handle;
            
            // header window
            
            attributes.X = 0;
            attributes.Y = 0;
            attributes.Width = Allocation.Width;
            attributes.Height = HeaderHeight;
            attributes.EventMask = (int)(
                Gdk.EventMask.ExposureMask |
                Gdk.EventMask.ScrollMask |
                Gdk.EventMask.ButtonPressMask |
                Gdk.EventMask.ButtonReleaseMask |
                Gdk.EventMask.KeyPressMask |
                Gdk.EventMask.KeyReleaseMask |
                Gdk.EventMask.PointerMotionMask |
                Events);
                
            header_window = new Gdk.Window(GdkWindow, attributes, attributes_mask);
            header_window.UserData = Handle;
            
            Style = Style.Attach(GdkWindow);
            GdkWindow.SetBackPixmap(null, false);
            bin_window.Background = Style.Base(State);
            Style.SetBackground(GdkWindow, StateType.Normal);
            Style.SetBackground(header_window, StateType.Normal);
            
            MoveResizeWindows(Allocation);
            
            graphics = new ListViewGraphics(this);
            graphics.RefreshColors();
        }
        
        protected override void OnUnrealized()
        {
            bin_window.UserData = IntPtr.Zero;
            bin_window.Destroy();
            bin_window = null;
            
            header_window.UserData = IntPtr.Zero;
            header_window.Destroy();
            header_window = null;
            
            base.OnUnrealized();
        }
        
        protected override void OnSizeRequested(ref Requisition requisition)
        {
            requisition.Width = width;
            requisition.Height = height + HeaderHeight;
            
            foreach(Widget child in Children) { 
                if(child.Visible) {
                    child.SizeRequest();
                }
            }
        }
        
        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            bool resized_width = Allocation.Width != allocation.Width;
            
            base.OnSizeAllocated(allocation);
            
            if(IsRealized) {
                GdkWindow.MoveResize(allocation);
                MoveResizeWindows(allocation);
            }
  
            row_area.Width = allocation.Width - (2 * BorderWidth);
            row_area.Height = allocation.Height;
           
            if(vadjustment != null) {
                vadjustment.PageSize = allocation.Height;
                vadjustment.PageIncrement = row_area.Height;
            }
            
            if(resized_width) {
                InvalidateHeaderWindow();
            }
            
            if(Model is ICareAboutView) {
                ((ICareAboutView)Model).RowsInView = RowsInView;
            }
            
            InvalidateBinWindow();
            RegenerateColumnCache();
        }
        
        protected override void OnSetScrollAdjustments(Adjustment hadj, Adjustment vadj)
        {
            if(vadj == null || hadj == null) {
                return;
            }
            
            vadj.ValueChanged += OnAdjustmentChanged;
            hadj.ValueChanged += OnAdjustmentChanged;
            
            UpdateAdjustments(hadj, vadj);
        }
        
        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {            
            foreach(Gdk.Rectangle rect in evnt.Region.GetRectangles()) {
                PaintRegion(evnt, rect);
            }
            
            return true;
        }
        
        protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
        {
            switch(evnt.Key) {
                case Gdk.Key.Up:
                    vadjustment.Value -= vadjustment.StepIncrement;
                    /*if((evnt.State & Gdk.ModifierType.ShiftMask) != 0) {
                        focus
                    }*/
                    
                    if(focused_row_index > 0) {
                        focused_row_index--;
                        InvalidateBinWindow();
                    }
                    
                    break;
                case Gdk.Key.Down:
                    vadjustment.Value += vadjustment.StepIncrement;
                    
                    if(focused_row_index < Model.Rows - 1) {
                        focused_row_index++;
                        InvalidateBinWindow();
                    }
                    
                    break;
                case Gdk.Key.Page_Up:
                    vadjustment.Value -= vadjustment.PageIncrement;
                    break;
                case Gdk.Key.Page_Down:
                    vadjustment.Value += vadjustment.PageIncrement;
                    break;
            }
            
            return base.OnKeyPressEvent(evnt);
        }
        
        protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        {
            HasFocus = true;
            
            if(evnt.Window == header_window) {
                Column column = GetColumnForResizeHandle((int)evnt.X);
                if(column != null) {
                    resizing_column_index = GetCachedColumnForColumn(column).Index;
                }
            } else if(evnt.Window == bin_window && model != null) {
                GrabFocus();
                    
                int row_index = GetRowAtY((int)evnt.Y);
                object item = model.GetValue(row_index);
                if(item == null) {
                    return true;
                }
                
                if((evnt.State & Gdk.ModifierType.ControlMask) != 0) {
                    Selection.ToggleSelect(row_index);
                    FocusRow(row_index);
                } else if((evnt.State & Gdk.ModifierType.ShiftMask) != 0) {
                    Selection.Clear();
                    Selection.SelectRange(Math.Min(focused_row_index, row_index), 
                        Math.Max(focused_row_index, row_index));
                } else {
                    Selection.Clear();
                    Selection.Select(row_index);
                    FocusRow(row_index);
                }
                
                InvalidateBinWindow();
            }
            
            return true;
        }
        
        protected override bool OnButtonReleaseEvent(Gdk.EventButton evnt)
        {
            if(evnt.Window == header_window) {
                if(resizing_column_index >= 0) {
                    resizing_column_index = -1;
                    header_window.Cursor = null;
                    return true;
                }
            
                Column column = GetColumnAt((int)evnt.X);
                if(column != null && Model is ISortable && column is ISortableColumn) {
                    ((ISortable)Model).Sort((ISortableColumn)column);
                    Model.Reload();
                    InvalidateHeaderWindow();
                }
            }
            
            return true;
        }
        
        protected override bool OnMotionNotifyEvent(Gdk.EventMotion evnt)
        {
            if(evnt.Window == header_window) {
                header_window.Cursor = resizing_column_index >= 0 || GetColumnForResizeHandle((int)evnt.X) != null ?
                    resize_x_cursor : null;
                  
                if(resizing_column_index >= 0) {
                    ResizeColumn(evnt.X);
                }
            }
            
            return true;
        }
        
        private void FocusRow(int index)
        {
            focused_row_index = index;
        }
        
        private void ResizeColumn(double x)
        {
            CachedColumn resizing_column = column_cache[resizing_column_index];

            double resize_delta = x - resizing_column.ResizeX2;
            double subsequent_columns = column_cache.Length - resizing_column.Index - 1;
            double even_distribution = 0.0;
            
            for(int i = 0; i <= resizing_column_index; i++) {
                even_distribution += column_cache[i].Column.Width * resize_delta;
            }

            even_distribution /= subsequent_columns;

            resizing_column.Column.Width = (resizing_column.Width + resize_delta) / (double)Allocation.Width;

            for(int i = resizing_column_index + 1; i < column_cache.Length; i++) {
                column_cache[i].Column.Width = (column_cache[i].Width - 
                    (column_cache[i].Column.Width * resize_delta) - 
                    even_distribution) / (double)Allocation.Width;
            }
            
            RegenerateColumnCache();
            InvalidateHeaderWindow();
            InvalidateBinWindow();
        }
        
        private void UpdateAdjustments(Adjustment hadj, Adjustment vadj)
        {
            if(vadj != null) {
                vadjustment = vadj;
            }
            
            if(vadjustment != null && model != null) {
                vadjustment.Upper = RowHeight * model.Rows + HeaderHeight;
                vadjustment.StepIncrement = RowHeight;
            }
            
            if(hadj != null) {
                hadjustment = hadj;
            }
            
            if(hadjustment != null) {
                hadjustment.Upper = 0;
                hadjustment.StepIncrement = 0;
            }
            
            vadjustment.Change();
        }
        
#endregion        
        
#region Drawing        
                
        private void PaintRegion(Gdk.EventExpose evnt, Gdk.Rectangle clip)
        {
            if(evnt.Window == header_window) {
                header_cr = CairoHelper.CreateCairoDrawable(header_window);
                header_cr.Rectangle(clip.X, clip.Y, clip.Width, clip.Height);
                header_cr.Clip();
                PaintHeader(evnt.Area);
            } else if(evnt.Window == bin_window) {
                bin_cr = CairoHelper.CreateCairoDrawable(bin_window);
                bin_cr.Rectangle(clip.X, clip.Y, clip.Width, clip.Height);
                bin_cr.Clip();
                PaintList(evnt, clip);
            } else if(evnt.Window == GdkWindow) {
                cr = CairoHelper.CreateCairoDrawable(GdkWindow);
                cr.Rectangle(clip.X, clip.Y, clip.Width, clip.Height);
                cr.Clip();
                PaintBorder(clip);
            }
        }
        
        private void PaintBorder(Gdk.Rectangle clip)
        {
            CairoExtensions.RoundedRectangle(cr, 0, 0, Allocation.Width, Allocation.Height, 10);
            cr.Color = graphics.ViewFill;
            cr.Fill();
            
            if(header_visible) {
                CairoExtensions.RoundedRectangle(cr, 0, 0, Allocation.Width, HeaderHeight + BorderWidth, 10,
                    CairoCorners.TopLeft | CairoCorners.TopRight);
                cr.Color = graphics.SelectionFill;
                cr.Fill();
            }
            
            CairoExtensions.RoundedRectangle(cr, 1, 1, Allocation.Width - 2, Allocation.Height - 2, 10);
            cr.Color = graphics.SelectionFill;
            cr.LineWidth = 2;
            cr.Stroke();
        }
        
        private void PaintHeader(Gdk.Rectangle clip)
        {
            header_cr.Rectangle(0, 0, Allocation.Width, Allocation.Height);
            header_cr.Color = graphics.SelectionFill;
            header_cr.Fill();
            
            if(column_controller == null) {
                return;
            }
                
            for(int ci = 0; ci < column_cache.Length; ci++) {            
                Gdk.Rectangle cell_area = new Gdk.Rectangle();
                cell_area.X = column_cache[ci].X1;
                cell_area.Y = column_text_y - 2;
                cell_area.Width = column_cache[ci].Width - COLUMN_PADDING;
                cell_area.Height = HeaderHeight - column_text_y;
                
                ColumnCell cell = column_cache[ci].Column.HeaderCell;
                
                if(cell is ColumnHeaderCellText && Model is ISortable) {
                    ((ColumnHeaderCellText)cell).HasSort = ((ISortable)Model).SortColumn 
                        == column_cache[ci].Column as ISortableColumn;
                }
                
                cell.Render(header_window, this, cell_area, cell_area, cell_area, StateType.Selected);
                
                header_cr.Color = graphics.ViewFill;
                header_cr.LineWidth = 1;
                
                if(ci < column_cache.Length - 1) {
                    header_cr.MoveTo(column_cache[ci].ResizeX1 - 1, cell_area.Y - 1);
                    header_cr.LineTo(column_cache[ci].ResizeX1 - 1, cell_area.Y + cell_area.Height - 2);
                    header_cr.Stroke();
                }
            }
        }
        
        private void PaintList(Gdk.EventExpose evnt, Gdk.Rectangle clip)
        {
            if(model == null) {
                return;
            }
            
            int rows = RowsInView;
            int first_row = (int)vadjustment.Value / RowHeight;
            int last_row = Math.Min(model.Rows, first_row + rows);

            for(int ri = first_row; ri < last_row; ri++) {
                Gdk.Rectangle single_row_area = new Gdk.Rectangle();
                single_row_area.Width = row_area.Width;
                single_row_area.Height = RowHeight;
                single_row_area.X = row_area.X;
                single_row_area.Y = row_area.Y + (ri * single_row_area.Height - (int)vadjustment.Value);
            
                StateType row_state = StateType.Normal;
                if(selection.Contains(ri)) {
                    row_state = StateType.Selected;
                }
                
                PaintRowBackground(ri, clip, single_row_area, row_state);
                PaintRowFocus(ri, clip, single_row_area, row_state);
                PaintRow(ri, clip, single_row_area, row_state);
            }
        }
        
        private void PaintRowFocus(int row_index, Gdk.Rectangle clip, Gdk.Rectangle area, StateType state)
        {
            if(row_index == focused_row_index && state != StateType.Selected) {
                Style.PaintFocus(Style, bin_window, State, clip, this, "row", area.X, area.Y, area.Width, area.Height);
            }
        }
        
        private void PaintRowBackground(int row_index, Gdk.Rectangle clip, Gdk.Rectangle area, StateType state)
        {
            if(row_index % 2 != 0 && rules_hint) {
                Style.PaintFlatBox(Style, bin_window, StateType.Normal, ShadowType.None, clip, this, "row",
                    area.X, area.Y, area.Width, area.Height);
            }
            
            if(state == StateType.Selected) {
                CairoExtensions.RoundedRectangle(bin_cr, area.X, area.Y, area.Width, area.Height, 4);
                bin_cr.Color = graphics.SelectionFill;
                bin_cr.Fill();
            }
        }
        
        private void PaintRow(int row_index, Gdk.Rectangle clip, Gdk.Rectangle area, StateType state)
        {
            if(column_cache == null) {
                return;
            }
            
            object item = model.GetValue(row_index);
            
            for(int ci = 0; ci < column_cache.Length; ci++) {
                Gdk.Rectangle cell_area = new Gdk.Rectangle();
                cell_area.Width = column_cache[ci].Width;
                cell_area.Height = RowHeight;
                cell_area.X = column_cache[ci].X1;
                cell_area.Y = area.Y;
                    
                PaintCell(item, ci, row_index, cell_area, clip, state);
            }
        }
        
        private void PaintCell(object item, int column_index, int row_index, Gdk.Rectangle area, 
            Gdk.Rectangle clip, StateType state)
        {
            ColumnCell cell = column_cache[column_index].Column.GetCell(0);
            cell.BindListItem(item);
            cell.Render(bin_window, this, area, area, area, state);
        }
        
        private void InvalidateBinWindow()
        {
            if(bin_window == null) {
                return;
            }
            
            int depth;
            Gdk.Rectangle rect = new Gdk.Rectangle();
            bin_window.GetGeometry(out rect.X, out rect.Y, out rect.Width, out rect.Height, out depth);
            rect.X -= BorderWidth;
            rect.Y -= HeaderHeight + BorderWidth;
            rect.Height += BorderWidth;
            bin_window.InvalidateRect(rect, false);
        }
        
        private void InvalidateHeaderWindow()
        {
            if(header_window == null) {
                return;
            }
            
            int depth;
            Gdk.Rectangle rect = new Gdk.Rectangle();
            header_window.GetGeometry(out rect.X, out rect.Y, out rect.Width, out rect.Height, out depth);
            rect.Y -= BorderWidth;
            rect.Height += BorderWidth;
            header_window.InvalidateRect(rect, false);
        }
        
#endregion
        
#region Row Utilities
        
        private int GetRowAtY(int y)
        {
            int page_offset = (int)vadjustment.Value % RowHeight;
            int first_row = (int)vadjustment.Value / RowHeight;
            int row_offset = (y + page_offset) / RowHeight;
            
            return first_row + row_offset;
        }
        
#endregion

#region Column Utilities

        private Column GetColumnForResizeHandle(int x)
        {
            if(column_cache == null) {
                return null;
            }
            
            foreach(CachedColumn column in column_cache) {
                if(x >= column.ResizeX1 - 2 && x <= column.ResizeX2 + 2) {
                    return column.Column;
                }
            }
            
            return null;
        }
        
        private Column GetColumnAt(int x)
        {
            if(column_cache == null) {
                return null;
            }
            
            foreach(CachedColumn column in column_cache) {
                if(x >= column.X1 && x <= column.X2) {
                    return column.Column;
                }
            }
            
            return null;
        }
        
        private CachedColumn GetCachedColumnForColumn(Column col)
        {
            foreach(CachedColumn ca_col in column_cache) {
                if(ca_col.Column == col) {
                    return ca_col;
                }
            }
            
            return CachedColumn.Zero;
        }

#endregion

#region Private Event Handlers

        private void OnAdjustmentChanged(object o, EventArgs args)
        {
            int ystep = (int)(vadjustment.Value - y_offset);
            int xstep = (int)(hadjustment.Value - x_offset);
            
            xstep = xstep > 0 ? Math.Max(xstep, Allocation.Width) : Math.Min(xstep, -Allocation.Width);
            ystep = ystep > 0 ? Math.Max(ystep, Allocation.Height) : Math.Min(ystep, -Allocation.Height);
            
            Gdk.Rectangle area;
            Gdk.Region offscreen = new Gdk.Region();
            
            area = new Gdk.Rectangle(
                Math.Max((int)(hadjustment.Value + 2 * xstep), 0),
                Math.Max((int)(vadjustment.Value + 2 * ystep), 0),
                Allocation.Width,
                Allocation.Height);
            offscreen.UnionWithRect(area);
        
            area = new Gdk.Rectangle(
                Math.Max((int)(hadjustment.Value + xstep), 0),
                Math.Max((int)(vadjustment.Value + ystep), 0),
                Allocation.Width,
                Allocation.Height);		
            offscreen.UnionWithRect(area);
            
            area = new Gdk.Rectangle(
                (int)hadjustment.Value,
                (int)vadjustment.Value,
                Allocation.Width,
                Allocation.Height);

            // always load the onscreen area last to make sure it
            // is first in the loading
            //Gdk.Region onscreen = Gdk.Region.Rectangle(area);
            //offscreen.Subtract(onscreen);
            //PreloadRegion(offscreen, ystep);
            //Preload(area, false);
            
            InvalidateBinWindow();
        }

#endregion

#region Model Event Handlers

        private void RefreshViewForModel()
        {
            UpdateAdjustments(null, null);
            vadjustment.Value = 0;
            
            if(Parent is ScrolledWindow) {
                Parent.QueueDraw();
            }
        }

        private void OnModelCleared(object o, EventArgs args)
        {
            RefreshViewForModel();
        }
        
        private void OnModelReloaded(object o, EventArgs args)
        {
            RefreshViewForModel();
        }

#endregion
        
#region Keyboard Shortcut Handlers
        
        private void SelectAll()
        {
            Selection.SelectRange(0, model.Rows, true);
            InvalidateBinWindow();
        }
        
        private void UnselectAll()
        {
            Selection.Clear();
            InvalidateBinWindow();
        }
        
#endregion

    }
}
