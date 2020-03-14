using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace VinPlaning
{
    /// <summary>
    /// Хранит виртуальное окно (Bitmap), занимающее всю клиентскую область родительского визуального компонента
    /// Автоматически обрабатывает событие Paint родительского визуального компонента (перерисовывает)
    /// Автоматически пересоздаёт окно в случае изменения клиентской области
    /// При изменении клиентского размера окна, вызывает из обработчика события Paint событие класса repainted
    /// Если isBackgroundPainting, то заранее закрашивает компонент (точнее, виртуальное окно) фоновым цветом backgroundColor перед вызовом обработчиков repainted
    /// Вызвать repaint(), чтобы вывести текущее виртуальное окно на экран
    /// </summary>
    public class VirtualWindow
    {
        public readonly Control parentControl;

        /// <summary>
        /// Создаёт одно виртуальное окно на весь компонент parent и регистрируется в обработчике paint
        /// </summary>
        /// <param name="parent">
        /// Компонент для отрисовки окна.
        /// Вызовите this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true) в компоненте.
        /// </param>
        public VirtualWindow(Control parent)
        {
            parentControl = parent;
            parentControl.Paint += new PaintEventHandler(parentControl_Paint);
        }

        public Bitmap vw = null;
        protected virtual void parentControl_Paint(object sender, PaintEventArgs e)
        {
            if (notCurrentVirtualWindow)
                return;

            if (vw == null || mustRepaint)
                setupVirtualWindowBitmap(e.Graphics);

            e.Graphics.DrawImageUnscaled(vw, 0, 0);
        }

        public bool  isBackgroundPainting = true;
        public Color backgroundColor = Color.White;
        private void setupVirtualWindowBitmap(Graphics g = null)
        {
            if (vw != null)
                vw.Dispose();

            vw = null;

            if (g == null)
                using (var cg = parentControl.CreateGraphics())
                {
                    vw = new Bitmap(parentControl.ClientSize.Width, parentControl.ClientSize.Height, cg);
                }
            else
                vw = new Bitmap(parentControl.ClientSize.Width, parentControl.ClientSize.Height, g);

            repaintImage();
        }

        /// <summary>
        /// Запрос полной перерисовки содержимого виртуального окна.
        /// Отрисовка repaint() на компоненте должна быть вызвана вручную.
        /// </summary>
        public void repaintImage()
        {
            if (!isBackgroundPainting && repainted == null)
                return;

            using (var ig = Graphics.FromImage(vw))
            {
                if (isBackgroundPainting)
                    ig.FillRectangle(new SolidBrush(backgroundColor), 0, 0, vw.Width, vw.Height);

                if (repainted != null)
                    repainted(vw, ig);
            }
        }

        /// <summary>
        /// Отрисовка виртуального окна на родительском компоненте
        /// </summary>
        public void repaint()
        {
            if (notCurrentVirtualWindow)
                return;

            parentControl.CreateGraphics().DrawImageUnscaled(vw, 0, 0);
        }

        /// <summary>
        /// Делегат, подписываемый на сообщение о необходимости перерисовки.
        /// </summary>
        /// <param name="vw">Содержимое рисунка виртуального окна типа Bitmap.</param>
        /// <param name="g">Объект Graphics, для рисования на виртуальном окне.</param>
        public delegate void repaintEvent(Bitmap vw, Graphics g);

        /// <summary>
        /// Подписка на сообщение о необходимости перерисовки виртуального окна.
        /// Вызывается в случае, если оно не отрисовано, либо его размеры изменены, либо вызвана repaintImage().
        /// </summary>
        public event repaintEvent repainted;

        /// <summary>
        /// Если true, окно не отрисовывает себя по repaint() и не обрабатывает Paint родительского компонента
        /// </summary>
        public bool notCurrentVirtualWindow = false;

        /// <summary>
        /// Сравнивает размер клиентской области окна с размером виртуального окна.
        /// Выдаёт true, если размеры не совпадают.
        /// </summary>
        public bool mustRepaint
        {
            get
            {
                return vw.Width != parentControl.ClientSize.Width || vw.Height != parentControl.ClientSize.Height;
            }
        }
    }
}
