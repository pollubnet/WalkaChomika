﻿#region License
/*
 * Written in 2014 by Marcin Badurowicz <m dot badurowicz at pollub dot pl>
 *
 * To the extent possible under law, the author(s) have dedicated
 * all copyright and related and neighboring rights to this 
 * software to the public domain worldwide. This software is 
 * distributed without any warranty. 
 *
 * You should have received a copy of the CC0 Public Domain 
 * Dedication along with this software. If not, see 
 * <http://creativecommons.org/publicdomain/zero/1.0/>.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WalkaChomika.Common.Models;
using WalkaChomika.Models;

namespace WalkaChomika
{
    /// <summary>
    /// To jest klasa odpowiadająca głównemu okienku aplikacji
    /// </summary>
    public partial class MainWindow : Window
    {
        private Zwierzę gracz1;
        private Zwierzę gracz2;

        /// <summary>
        /// Konstruktor klasy głównego okienka aplikacji
        /// </summary>
        public MainWindow()
        {
            // to jest standardowa metoda ustawiająca komponenty
            InitializeComponent();

            // tutaj się dzieje nieistotna magia - przekierowywany jest strumień informacji testowych
            // do pola tekstowego w okienku aplikacji
            TraceListener debugListener = new Ktos.Common.TextBoxTraceListener(tbLog);
            Debug.Listeners.Add(debugListener);

            // tworzone są nowe instancje walczących zwierzątek
            ////gracz1 = new ChomikSzaman("Pimpuś", 100);
            gracz1 = new ArmiaChomików(300);
            gracz2 = new Jednorożec("Rafał", 15);

            // subskrybujemy zdarzenie Zmarł - kiedy jednorożec padnie,
            // będziemy pokazywać komunikat
            // tutaj użyta jest wersja z funkcją anonimową
            gracz2.Zmarł += (sender) =>
            {
                MessageBox.Show(string.Format("{0} nie żyje!", sender.Imię));
            };

            // wersja alternatywna - z oddzielną funkcją obsługującą zdarzenie
            gracz2.Zmarł += Gracz2_Zmarł;
        }

        /// <summary>
        /// Funkcja obsługująca zdarzenie śmierci jednego z graczy
        /// </summary>
        /// <param name="sender">Obiekt, który wysłał zdarzenie Zmarł</param>
        private void Gracz2_Zmarł(Zwierzę sender)
        {
            MessageBox.Show(string.Format("{0} nie żyje!", sender.Imię));
        }

        /// <summary>
        /// Definiuje, czy ostatnią turę miał gracz1, czy graczDrugi
        /// </summary>
        private bool _lastGracz;

        /// <summary>
        /// Funkcja obsługująca naciśnięcie przycisku Następnej Tury
        /// </summary>
        /// <param name="sender">Obiekt, który uruchomił zdarzenie</param>
        /// <param name="e">Parametry zdarzenia</param>
        private void NextTurnClick(object sender, RoutedEventArgs e)
        {
            if (_lastGracz)
                Tura(gracz1, gracz2);
            else
                Tura(gracz2, gracz1);

            PokażStan();

            if (!gracz1.CzyŻyje() || !gracz2.CzyŻyje())
            {
                btnNextTurn.IsEnabled = false;
            }

            _lastGracz = !_lastGracz;
        }

        /// <summary>
        /// Pokazywanie stanu gracza - czy żyje, czy nie, jakie ma statystyki
        /// </summary>
        private void PokażStan()
        {
            if (!gracz1.CzyŻyje() || !gracz2.CzyŻyje())
            {
                Debug.WriteLine(gracz1.CzyŻyje() ? gracz1.Imię + " wygrał!" : gracz2.Imię + " wygrał!");
            }
            else
            {
                Debug.WriteLine("Gracz 1: {0}", gracz1);
                Debug.WriteLine("Gracz 2: {0}", gracz2);
            }

            scroll.ScrollToBottom();
        }

        /// <summary>
        /// Obsługa kolejnej tury - gracz atakuje graczDrugi, losując typ ataku w zależności od jego umiejętności
        /// </summary>
        /// <param name="gracz">Gracz atakujący</param>
        /// <param name="graczDrugi">Gracz atakowany</param>
        private void Tura(Zwierzę gracz, Zwierzę graczDrugi)
        {
            Random r = new Random();
            var w = r.Next(10);
            var zaatakował = false;

            if (gracz is ZwierzęMagiczne)
            {
                // 30% szans na atak magiczny
                if (w >= 7)
                {
                    (gracz as ZwierzęMagiczne).AtakujMagicznie(graczDrugi);
                    Debug.WriteLine("{0} zaatakował magicznie {1}!", gracz.Imię, graczDrugi.Imię);
                    zaatakował = true;
                }
            }

            if (gracz is ILatający)
            {
                // 20% szans na odlot
                if (w >= 8 && !zaatakował)
                {
                    try
                    {
                        (gracz as ILatający).Lataj();
                        Debug.WriteLine(string.Format("{0} odleciał!", gracz.Imię));
                    }
                    catch (HorseCannotIntoSkyException) // jeżeli wystąpił wyjątek, że koń nie może polecieć
                    {
                        Debug.WriteLine("Koń za słaby, aby latać!"); // pokazujemy komunikat
                    }
                    finally
                    {
                        zaatakował = true; // niezależnie, czy poleciał, czy nie, uznajemy, że jego tura się kończy
                    }
                }
            }

            // jeżeli nie zaatakował wcześniej w inny sposób, to po prostu gryzie
            if (!zaatakował)
            {
                gracz.Gryź(graczDrugi);
                Debug.WriteLine("{0} ugryzł {1}!", gracz.Imię, graczDrugi.Imię);
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                try
                {
                    gracz2.Imię = ((System.Windows.Controls.TextBox)sender).Text;
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Imię złe!");
                }
            }
        }

        private void SaveFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText("log.txt", tbLog.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu: " + ex.Message);
            }
        }
    }
}