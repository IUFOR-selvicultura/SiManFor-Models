using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;

namespace EngineTest
{
    /// <summary>
    /// Todas las funciones y procedimientos son opcionales. Si se elimina cualquiera de ellas, se usará un
    /// procedimiento o funcion por defecto que no modifica el estado del inventario.
    /// Modelo de árbol individual para repoblaciones de Pinus halepensis en Aragón (föra forest techonlogies y Diputación General de Aragón)
    /// </summary>
    public class Template : ModelBase
    {
        /// declaracion de variables publicas
        public PieMayor currentTree;

        /// Funciones de perfil utilizadas en el cálculo de volumenes
        /// 
        public double r2_conCorteza(double HR)///HR es la altura relativa, en tanto por uno
        {
            double r = MyFunc.perfil(HR, currentTree.ALTURA.Value, currentTree.DAP.Value) / 2; ///radio en cm
            return Math.Pow(r, 2);
        }

        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
        /// Solo se ejecuta en el primer nodo. 
        /// Variables que deben permanecer constantes como el índice de sitio deben calcularse sólo en este apartado del modelo
        /// </summary>
        /// <param name="plot"></param>
        public override void CalculateInitialInventory(Parcela plot)
        {
            IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));

            double bal = 0;
            double old_sec_normal = 100000;
            double old_bal = 0;
            //double sec_normal = tree.SEC_NORMAL;
			
            ///índice de sitio basado en la ecuación Ho-t para Pinus halepensis en el Valle del Ebro (GADA) (Rojo etal. 2017), edad de referencia 60 años
            plot.SI = MyFunc.Ecuacion_rojo_alboreca(plot.H_DOMINANTE.Value, plot.EDAD.Value, 60);

            foreach (PieMayor tree in piesOrdenados)
            {
                if (old_sec_normal > tree.SEC_NORMAL)
                {
                    tree.BAL = bal;
                    old_bal = bal;
                }
                else
                {
                    tree.BAL = old_bal;
                }

                bal += tree.SEC_NORMAL.Value * tree.EXPAN.Value / 10000;

                old_sec_normal = tree.SEC_NORMAL.Value;

                if (!tree.ALTURA.HasValue)
                {
                    tree.ALTURA = MyFunc.alturaDiametroGeneralizada(plot.H_DOMINANTE.Value, tree.DAP.Value, plot.D_DOMINANTE.Value);///altura-diámetro generalizada. Alturas en m y diámetros en cm =======
                }

                if (!tree.ALTURA_BC.HasValue)
                {
                    tree.ALTURA_BC = MyFunc.alturaInicioCopa(tree.ALTURA.Value, plot.I_HART.Value / 100, plot.SI.Value, (tree.BAL.Value / plot.A_BASIMETRICA.Value));///altura de la base de la copa (altura de inicio de copa viva). Alturas en m y diámetros en cm
                }

                tree.LCW = MyFunc.diametroCopa(tree.DAP.Value, tree.ALTURA.Value);///diámetro de copa (m)

                currentTree = tree;
                tree.VCC = Math.PI * 0.0001 * tree.ALTURA.Value * IntegralBySimpson(0, 1, 0.01, r2_conCorteza); //Integración --> r2_conCorteza sobre HR en los limites 0 -> 1 // VCC en m³
                currentTree = null;		
            }			
        }

        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
        /// </summary>
        public override void Initialize(Parcela plot)
        {
        }

        /// <summary>
        /// Procedimiento que permite la inicialización de variables de árbol necesarias para la ejecución del modelo
        /// </summary>
        public override void InitializeTree(Parcela plot, PieMayor tree)
        {
        }

        /// <summary>
        /// Función que indica si el árbol sobrevive o no después de "years" años
        /// </summary>
        /// <returns>Devuelve el porcentaje de árboles que sobreviven (que luego multiplica por EXPAN)
        public override double Survives(double years, Parcela plot, PieMayor tree)
        {
			double prob = MyFunc.probabilidadSupervivenciaen10(plot.A_BASIMETRICA.Value, tree.BAL.Value / plot.A_BASIMETRICA.Value, plot.I_HART.Value / 100);
            double prob_years = Math.Pow(prob,years/10);///el exponente years/10 es para interpolar a "years" años, ya que el modelo de supervivencia se construyó para un lapso de 10 años
            return prob_years;
        }

        /// <summary>
        /// Procedimiento que permite modificar las propiedades del árbol durante su crecimiento después de "years" años (LAPSOS DE 5 AÑOS)
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="oldTree"></param>
        /// <param name="newTree"></param>
        public override void Grow(double years, Parcela plot, PieMayor oldTree, PieMayor newTree)
        {
            double id10 = MyFunc.incrementoDiametroConCortezaen10(oldTree.DAP.Value, plot.A_BASIMETRICA.Value, plot.SI.Value, oldTree.BAL.Value / plot.A_BASIMETRICA.Value);
            newTree.DAP = oldTree.DAP + id10*(years/10);///el factor years/10 es para interpolar a "years" años, ya que el modelo de incremento diametral se construyó para un lapso de 10 años
        }

        /// <summary>
        /// Procedimiento que permite añadir nuevos árboles a una parcela después de "years" años
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <returns>Area basimetrica a distribuir o 0 si no hay masa incorporada</returns>
        public override double? AddTree(double years, Parcela plot)
        {
            return 0.0F;///no hay incorporación de masa en este modelo
        }

        /// <summary>
        /// Expresa como se ha de distribuir la masa incorporada entre los árboles existentes.
        /// La implementación por defecto la distribuye de forma uniforme.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="AreaBasimetricaIncorporada"></param>
        /// <returns></returns>
        public override Distribution[] NewTreeDistribution(double years, Parcela plot, double AreaBasimetricaIncorporada)///no hay incorporación de masa en este modelo
        {
            /// Hay que definir una matriz (distribution) que pertenece a la clase Distribution
            /// que tiene 3 propiedades: diametro menor (.diametroMenor), diametro mayor (.diametroMayor)
            /// y área basimetrica que se añadira al rango diamétrico (.AreaBasimetricaToAdd)

            Distribution[] distribution = new Distribution[3];
            double percentAreaBasimetrica = AreaBasimetricaIncorporada / plot.A_BASIMETRICA.Value;
            distribution[0] = new Distribution();
            distribution[0].diametroMenor = 0.0;
            distribution[0].diametroMayor = 12.5;
            distribution[0].AreaBasimetricaToAdd = 0.0384 * AreaBasimetricaIncorporada;
            distribution[1] = new Distribution();
            distribution[1].diametroMenor = 12.5;
            distribution[1].diametroMayor = 22.5;
            distribution[1].AreaBasimetricaToAdd = 0.2718 * AreaBasimetricaIncorporada;
            distribution[2] = new Distribution();
            distribution[2].diametroMenor = 22.5;
            distribution[2].diametroMayor = double.MaxValue;
            distribution[2].AreaBasimetricaToAdd = 0.6898 * AreaBasimetricaIncorporada;
            return distribution;
        }

        /// <summary>
        /// Procedimiento que realiza todos los precálculos para preparar el procesamiento de los árboles y parcelas.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="trees"></param>
        public override void PreCalculation(double years, Parcela plot, PieMayor[] trees)
        {
        }

		        /// <summary>
        /// Procedimiento que realiza los cálculos sobre un árbol.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="tree"></param>
        public override void ProcessTree(double years, Parcela plot, PieMayor tree)
        {
            ///currentTree = tree;
            ///tree.VCC = Math.PI * 0.0001 * tree.ALTURA.Value * IntegralBySimpson(0, 1, 0.01, r2_conCorteza);
            ///currentTree = null;
        }

        /// <summary>
        /// Procedimiento que realiza los cálculos sobre una parcela.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="trees"></param>
        public override void ProcessPlot(double years, Parcela plot, PieMayor[] trees)
        {
            ///recalcula la Ho para la nueva edad basándose en el índice de sitio
            if (plot.VAR_5.HasValue == false) { plot.VAR_5 = plot.EDAD.Value; }
            plot.VAR_5 += years;
            plot.H_DOMINANTE = MyFunc.Ecuacion_rojo_alboreca(plot.SI.Value, 60, plot.VAR_5.Value);
			///recalcula IH
			plot.I_HART = 10000 / (plot.H_DOMINANTE.Value * Math.Sqrt(plot.N_PIESHA.Value));

            IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
            double bal = 0; double old_sec_normal = 100000; double old_bal = 0;
            foreach (PieMayor tree in piesOrdenados)
            {
                if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
                {
                    if (old_sec_normal > tree.SEC_NORMAL)
                    {
                        tree.BAL = bal;
                        old_bal = bal;
                    }
                    else
                    {
                        tree.BAL = old_bal;
                    }
                    bal += tree.SEC_NORMAL.Value * tree.EXPAN.Value / 10000;
                    old_sec_normal = tree.SEC_NORMAL.Value;

                    tree.ALTURA = MyFunc.alturaDiametroGeneralizada(plot.H_DOMINANTE.Value, tree.DAP.Value, plot.D_DOMINANTE.Value);
                    tree.ALTURA_BC = MyFunc.alturaInicioCopa(tree.ALTURA.Value, plot.I_HART.Value / 100, plot.SI.Value, (tree.BAL.Value / plot.A_BASIMETRICA.Value));
                    tree.LCW = MyFunc.diametroCopa(tree.DAP.Value, tree.ALTURA.Value);
                }
                else { tree.BAL = 0; tree.CR = 0; tree.LCW = 0; tree.ALTURA_MAC = 0; tree.ALTURA_BC = 0; }
            }
			
			///recalcula correctamente la altura media, diámetros de copa medios, FCC y VCC por hectárea
			double exp_temp = 0;
			double h_exp_temp = 0;
			double dcopa_exp_temp = 0;
			double dcopa2_exp_temp = 0;
			double vcc_exp_temp = 0;
			
			foreach (PieMayor tree in piesOrdenados)
            {
				if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
				{
					exp_temp = exp_temp + (tree.EXPAN.Value);
					h_exp_temp = h_exp_temp + (tree.ALTURA.Value * tree.EXPAN.Value);
					dcopa_exp_temp = dcopa_exp_temp + (tree.LCW.Value * tree.EXPAN.Value);
					dcopa2_exp_temp = dcopa2_exp_temp + (Math.Pow(tree.LCW.Value,2) * tree.EXPAN.Value);
					
					currentTree = tree;
					tree.VCC = Math.PI * 0.0001 * tree.ALTURA.Value * IntegralBySimpson(0, 1, 0.01, r2_conCorteza);///recalcula el vcc de cada árbol con la altura correcta
					currentTree = null;
					vcc_exp_temp = vcc_exp_temp + (tree.VCC.Value * tree.EXPAN.Value);
				}
            }
			
			///double h_media_new = h_exp_temp / exp_temp;
			plot.H_MEDIA = 	h_exp_temp / exp_temp;
			plot.DM_COPA = dcopa_exp_temp / exp_temp;
			plot.DG_COPA = Math.Sqrt(dcopa2_exp_temp / exp_temp);
			plot.VCC = vcc_exp_temp;
			plot.FCC = 0.25 * Math.PI * dcopa2_exp_temp / 10000;
			            
        }
		
    }


    ///CLASE CON LA FORMULACIÓN DE LAS ECUACIONES PROPIAS DEL MODELO
	///los árboles de clasificación para los modelos de regeneración no están aquí, se ejecutan en VBA en el xlsm de salida
    public static class MyFunc
    {
        /// <summary>
		///ecuación Ho-t para Pinus halepensis en el Valle del Ebro (GADA) (Rojo etal. 2017)
        /// H1 y H2, las alturas dominantes a las edades t1 y t2, respectivamente.
        /// El índice de sitio se define como el valor de la altura dominante a la edad de referencia de 60 años.
        /// en cada nodo, t1=EDAD y t2=t1+years 
        /// </summary>
        /// <param name="H_1">Altura inicial, Simanfor: H_DOMINANTE (m)</param>
        /// <param name="t1">t1 EDAD en años </param>
        /// <param name="t2">t2 EDAD + years en años </param>
        /// <returns>Nueva altura. Simanfor ALTURA</returns>
        public static double Ecuacion_rojo_alboreca(double H_1, double t1, double t2)
        {
            double r = Math.Sqrt(Math.Pow(H_1 - 9.5968, 2) + 4 * 2046.311 * H_1 * Math.Pow(t1, -1.3097));
            double h2 = (H_1 + 9.5968 + r) / (2 + 4 * 2046.311 * Math.Pow(t2, -1.3097) / (H_1 - 9.5954 + r));
            return h2;
        }

        /// <summary>
        /// función que calcula el diámetro normal con corteza en función del diámetro sin corteza (en las mismas unidades el uno que el otro)
        /// </summary>
        /// <param name="dsc_cm">Diámetro normal sin corteza (cm). Simanfor: NO TIENE ESTA VARIABLE, TIENE "CORTEZA";
		/// <returns></returns>
        public static double Eq_dn(double dsc_cm)
        {
            double dn_cm = 1.0936 * dsc_cm;
            return dn_cm;
        }

        /// <summary>
        /// función que calcula el diámetro máximo de copa en m
        /// </summary>
        /// <param name="dn_cm">Diámetro normal con corteza	(cm). Simanfor: DAP (cm) </param>
        /// <param name="ht_m">Altura total	(m). Simanfor: ALTURA (m)</param>
        /// <returns></returns>
        public static double diametroCopa(double dn_cm, double ht_m)
        {
            return 0.672001 * Math.Pow(dn_cm, 0.880032) * Math.Pow(ht_m, -0.60344) * Math.Exp(0.057872 * ht_m);
        }

        /// <summary>
        /// función que calcula la altura del inicio de la copa viva en m
        /// </summary>
        /// <param name="ht_m">Altura total	m. Simanfor: ALTURA (m)</param>
        /// <param name="IH">Índice de Hart-Becking (marco real)	tanto por uno. Simanfor: I_HART (%) </param>
        /// <param name="IS">Índice de sitio según Rojo-Alboreca et al. 2017 (edad de referencia=60 años)	m. Simanfor: SI (m)</param>
        /// <param name="BALMOD">BAL/G, adimensional. Simanfor: BAL (m2/ha de los árboles más gruesos que el árbol en ejecución)</param>
        /// <returns></returns>
        public static double alturaInicioCopa(double ht_m, double IH, double IS, double BALMOD)
        {
            double hicv_m = ht_m / (1 + Math.Exp(-0.82385 + 4.039408 * IH - 0.01969 * IS - 0.594323 * BALMOD));
            return hicv_m;
        }

        /// <summary>
        /// función altura-diámetro generalizada (calcula la altura de cada árbol en función de su diámetro y de Ho y Do)
        /// </summary>
        /// <param name="H0">Altura dominante (Assmann)	m. Simanfor H_DOMINANTE (m) </param>
        /// <param name="dn_cm">Diámetro normal con corteza	cm. Simanfor: DAP (cm)</param>
        /// <param name="Do">Diámetro dominante (Assmann)	cm. Simanfor: D_DOMINANTE (cm)</param>
        /// <returns>Altura total	m. Simanfor: ALTURA (m)</returns>
        public static double alturaDiametroGeneralizada(double H0, double dn_cm, double Do)
        {
            double a = 2.5511;
            double b = Math.Pow(1.3, a);
            double ht_m = Math.Pow(b + (Math.Pow(H0, a) - b) * (1 - Math.Exp(-0.025687 * dn_cm)) / (1 - Math.Exp(-0.025687 * Do)), 1.0 / a);
            return ht_m;
        }

        /// <summary>
        /// función que calcula el incremento diametral con corteza para el siguiente periodo de 10 años
        /// </summary>
        /// <param name="dn_cm">Diámetro normal con corteza	cm. Simanfor: DAP (cm)</param>
        /// <param name="G">área basimétrica (m²/ha). Simanfor: A_BASIMETRICA (m²/ha)</param>
        /// <param name="SI">Índice de sitio según Rojo-Alboreca et al. 2017 (m) (edad de referencia=60 años)	m. Simanfor: SI (m)</param>
        /// <param name="BALMOD">BAL/G, adimensional. Simanfor: BAL (m2/ha de los árboles más gruesos que el árbol en ejecución)</param>
        /// <returns></returns>
        public static double incrementoDiametroConCortezaen10(double dn_cm, double G, double SI, double BALMOD)
        {
            double id10 = 0.906633 * Math.Exp(0.09701 * dn_cm - 0.00111 * dn_cm * dn_cm - 0.05201 * G + 0.050652 * SI - 0.09366 * BALMOD);
            return id10;
        }

        /// <summary>
        /// @@@función que calcula la probabilidad de que un árbol sobreviva al siguiente periodo de 10 años
		/// @@@ojo, como hay que interpolar a cinco años, lo que debe devolver la función al final es pi^0.5
        /// </summary>
        /// <param name="G">Área basimétrica	m2/ha. Simanfor: A_BASIMETRICA (m2/ha)</param>
        /// <param name="BALMOD">BAL/G. Simanfor: BAL (m2/ha de los árboles más gruesos que el árbol en ejecución)</param>
        /// <param name="IH">Índice de Hart-Becking (marco real)	tanto por uno. Simanfor: I_HART (%)</param>
        /// <returns></returns>
        public static double probabilidadSupervivenciaen10(double G, double BALMOD, double IH)
        {
			double probabilidad = 1 / (1 + Math.Exp(-6.5934 + 0.0305 * G + 5.6845 * BALMOD - 8.1523 * IH));
            return probabilidad;
        }

        /// <summary>
        /// función de perfil (STUD) que devuelve el diámetro (cm) a la altura hi (m)
		/// se integra numéricamente para calcular el volumen con corteza
        /// </summary>
        /// <param name="q">Altura HR</param>
        /// <param name="ht_m">Altura total	m. Simanfor: ALTURA (m)</param>
        /// <param name="dn_cm">Diámetro normal con corteza	cm. Simanfor: DAP (cm)</param>
        /// <returns> di diametro cm con corteza a la altura hi Simanfor: </returns>
        public static double perfil(double q, double ht_m, double dn_cm)
        {
            //double q = hi / ht_m;
            double E = 100 * ht_m / dn_cm;
            double di = (1 + 1.121163 * Math.Exp(-10.23293 * q)) * 0.696362 * dn_cm * Math.Pow(1 - q, 1.266261 - (0.003553 * E) - 1.865418 * (1 - q));
            return di;
        }

    }
}
