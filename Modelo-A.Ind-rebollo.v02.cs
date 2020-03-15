//// modelo de silvestre para usar como plantilla
//// modelo de arbol individual
using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
namespace EngineTest
{
    /// <summary>
    /// Modelo para masas de Quercus pyrenaica basado en los artículos:
    /// Adame P; Cañellas I; Roig S; del Río Gaztelurrutia M; 2006;"Modelling dominant height growth and site index curves for rebollo oak (Quercus pyrenaica Willd.)
    /// Adame P; del Río Gaztelurrutia M; Cañellas I; 200;"Modelling dominant height growth and site index curves for rebollo oak (Quercus pyrenaica Willd.)
    /// Rodriguez P; Cubifor; modelo de cubicación    
    /// </summary>
    public class Template : ModelBase
    {
        /// declaracion de variables publicas
        public PieMayor currentTree;
        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
		/// Solo se ejecuta en el primer nodo. 
		/// Variables que deben permanecer constantes como el índice de sitio deben calcularse solo en este apartado del modelo
        /// </summary>
        /// <param name="plot"></param>
        public override void CalculateInitialInventory(Parcela plot)
        {
            double w_s_b7=0; double w_b27=0; double w_b2=0; double w_r=0 ;
			foreach (PieMayor tree in plot.PiesMayores)
            {
                tree.BAL = 0;
                foreach (PieMayor pm in tree.Parcela.PiesMayores)
                {
                    if (pm.DAP>tree.DAP)
                    {
                        tree.BAL+=pm.SEC_NORMAL.Value*pm.EXPAN.Value/10000;
                    }
                }
            	if (!tree.ALTURA.HasValue)
            	{
					tree.ALTURA = 1.3 + (3.099 - 0.00203*plot.A_BASIMETRICA.Value + 1.02491*plot.H_DOMINANTE.Value * Math.Exp(-8.5052/tree.DAP.Value));
            	}
            	//if (!tree.ALTURA_MAC.HasValue)
            	//{
				//	tree.ALTURA_MAC = tree.ALTURA.Value / (1 + Math.Exp((double)(-0.0012*tree.ALTURA.Value*10-0.0102*tree.BAL.Value-0.0168*plot.A_BASIMETRICA.Value )));
                //}
            	//if (!tree.ALTURA_BC.HasValue)
            	//{
				//	tree.ALTURA_BC = tree.ALTURA_MAC.Value / (1+Math.Exp((double)(1.2425*(plot.A_BASIMETRICA.Value/(tree.ALTURA.Value*10)) + 0.0047*(plot.A_BASIMETRICA.Value) - 0.5725*Math.Log(plot.A_BASIMETRICA.Value)-0.0082*tree.BAL.Value)));
            	//}
            	//tree.CR=1-tree.ALTURA_BC.Value/tree.ALTURA.Value;
            	//if (!tree.LCW.HasValue)
            	//{
				//	tree.LCW=(1/10.0F)*(0.2518*tree.DAP.Value*10)*Math.Pow(tree.CR.Value,(0.2386+0.0046*(tree.ALTURA.Value-tree.ALTURA_BC.Value)*10));
            	//}
                tree.VCC=0.000051*Math.Pow(tree.DAP.Value,1.86781)*Math.Pow(tree.ALTURA.Value,0.989625);
				/// almacenamiento de variables de biomasa (eq. para Q. pyrenaica, Ruiz-Peinado et al 2011)
				if (tree.ESPECIE==43)
				{
				/// Stem + Thick branches: Ws + Wb7 = 0.0261 · d2 · h
				tree.VAR_5 = 0.0261 * Math.Pow(tree.DAP.Value,2) * tree.ALTURA.Value; w_s_b7+=tree.VAR_5.Value*tree.EXPAN.Value;
				/// Medium branches: Wb2–7 = –0.0260 · d2 + 0.536 · h + 0.00538 · d2 · h 
				tree.VAR_2 = -0.0260 * Math.Pow(tree.DAP.Value,2) + 0.536 * tree.ALTURA.Value + 0.00538 * Math.Pow(tree.DAP.Value,2) * tree.ALTURA.Value; w_b27+=tree.VAR_2.Value*tree.EXPAN.Value;
				/// Thin branches: Wb2 = 0.898 · d – 0.445 · h  
				tree.VAR_3 = 0.898 * tree.DAP.Value - 0.445 * tree.ALTURA.Value; w_b2+=tree.VAR_3.Value*tree.EXPAN.Value;
				/// Roots: Wr = 0.143 · d2 
				tree.VAR_4 = 0.143 * Math.Pow(tree.DAP.Value,2); w_r+=tree.VAR_4.Value*tree.EXPAN.Value;
				}
            }
        ///     Calculo del indice de sitio: SI
        double parA1, parA2, parA3, parA4, t1, t2, X, H0, H2;
        parA1   = 15.172;
        parA2   = -4.2126;
        parA3   = 0.1439;
        parA4   = 0.6711;
        H0  = plot.H_DOMINANTE.Value;
        t2  = 1-Math.Exp((double) -parA3*Math.Pow(60,parA4));
        t1  = 1-Math.Exp((double) -parA3*Math.Pow(plot.EDAD.Value,parA4));
        X   = (Math.Log(H0)-parA1*Math.Log(t1))/(1+parA2*Math.Log(t1));
        H2  = Math.Exp(X) * Math.Pow(t2,parA1+parA2*X);
        plot.SI     = H2;
        plot.VAR_1  = X;
		plot.VAR_2  = w_b27/1000;
		plot.VAR_3  = w_b2/1000;
		plot.VAR_4  = w_r/1000;
		plot.VAR_5  = w_s_b7/1000;
        }

        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
        /// </summary>
        /// <param name="plot"></param>
        public override void Initialize(Parcela plot)
        {
            double w_s_b7=0; double w_b27=0; double w_b2=0; double w_r=0 ;
			foreach (PieMayor tree in plot.PiesMayores)
            {
				if (tree.ESPECIE==43)
				{
				w_s_b7+=tree.VAR_5.Value*tree.EXPAN.Value;
				w_b27+=tree.VAR_2.Value*tree.EXPAN.Value;
				w_b2+=tree.VAR_3.Value*tree.EXPAN.Value;
				w_r+=tree.VAR_4.Value*tree.EXPAN.Value;	
				}
			}
			plot.VAR_2  = w_b27/1000;
			plot.VAR_3  = w_b2/1000;
			plot.VAR_4  = w_r/1000;
			plot.VAR_5  = w_s_b7/1000;			
        }
        /// <summary>
        /// Procedimiento que permite la inicialización de variables de árbol necesarias para la ejecución del modelo
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="tree"></param>
        public override void InitializeTree(Parcela plot, PieMayor tree)
        {
            if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
            {
                tree.BAL = 0;
                foreach (PieMayor pm in tree.Parcela.PiesMayores)
                {
                    if (!pm.ESTADO.HasValue || String.IsNullOrEmpty(pm.ESTADO.ToString()))
                    {
                        if (pm.DAP > tree.DAP)
                        {
                            tree.BAL+=pm.SEC_NORMAL.Value*pm.EXPAN.Value/10000;
                        }
                    }
                }
            }
            if (!tree.ALTURA.HasValue)
            {
                tree.ALTURA = 1.3 + (3.099 - 0.00203*plot.A_BASIMETRICA.Value + 1.02491*plot.H_DOMINANTE.Value * Math.Exp(-8.5052/tree.DAP.Value));
            }
        //    	if (!tree.ALTURA_MAC.HasValue)
        //    	{
		//			tree.ALTURA_MAC = tree.ALTURA.Value / (1 + Math.Exp((double)(-0.0012*tree.ALTURA.Value*10-0.0102*tree.BAL.Value-0.0168*plot.A_BASIMETRICA.Value )));
        //    	}
        //    	if (!tree.ALTURA_BC.HasValue)
        //    	{
		//			tree.ALTURA_BC = tree.ALTURA_MAC.Value / (1+Math.Exp((double)(1.2425*(plot.A_BASIMETRICA.Value/(tree.ALTURA.Value*10)) + 0.0047*(plot.A_BASIMETRICA.Value) - 0.5725*Math.Log(plot.A_BASIMETRICA.Value)-0.0082*tree.BAL.Value)));
        //    	}
        //    	tree.CR=1-tree.ALTURA_BC.Value/tree.ALTURA.Value;
        //    	if (!tree.LCW.HasValue)
        //    	{
		//			tree.LCW=(1/10.0F)*(0.2518*tree.DAP.Value*10)*Math.Pow(tree.CR.Value,(0.2386+0.0046*(tree.ALTURA.Value-tree.ALTURA_BC.Value)*10));
        //    	}
                tree.VCC=0.000051*Math.Pow(tree.DAP.Value,1.86781)*Math.Pow(tree.ALTURA.Value,0.989625);
				/// almacenamiento de variables de biomasa (eq. para Q. pyrenaica, Ruiz-Peinado et al 2011)
				if (tree.ESPECIE==43)
				{
				/// Stem + Thick branches: Ws + Wb7 = 0.0261 · d2 · h
				tree.VAR_5 = 0.0261 * Math.Pow(tree.DAP.Value,2) * tree.ALTURA.Value;
				/// Medium branches: Wb2–7 = –0.0260 · d2 + 0.536 · h + 0.00538 · d2 · h 
				tree.VAR_2 = -0.0260 * Math.Pow(tree.DAP.Value,2) + 0.536 * tree.ALTURA.Value + 0.00538 * Math.Pow(tree.DAP.Value,2) * tree.ALTURA.Value;
				/// Thin branches: Wb2 = 0.898 · d – 0.445 · h  
				tree.VAR_3 = 0.898 * tree.DAP.Value - 0.445 * tree.ALTURA.Value;
				/// Roots: Wr = 0.143 · d2 
				tree.VAR_4 = 0.143 * Math.Pow(tree.DAP.Value,2);
				}
        }
        /// <summary>
        /// Función que indica si el árbol sobrevive o no después de "years" años
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="tree"></param>
        /// <returns>Devuelve el porcentaje de árboles que sobreviven</returns>
        public override double Survives(double years, Parcela plot, PieMayor tree)
        {
		double p_mort = 1/(1 + Math.Exp(1.3286 - 9.791/tree.DAP.Value + 3.5383*tree.ALTURA.Value/plot.H_DOMINANTE.Value) );
	    return 1-p_mort;
        }
        /// <summary>
        /// Procedimiento que permite modificar las propiedades del árbol durante su crecimiento después de "years" años
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="oldTree"></param>
        /// <param name="newTree"></param>
        public override void Grow(double years, Parcela plot, PieMayor oldTree, PieMayor newTree)
        {
        double STR = 0; // su valor debe ser 1 cuando la masa esta en el estrato 1
		double DBHG5 = Math.Exp(0.8351 + 0.1273*Math.Log(oldTree.DAP.Value) - 0.00006*Math.Pow(oldTree.DAP.Value,2)
            - 0.01216 * oldTree.BAL.Value - 0.00016 * plot.N_PIESHA.Value - 0.03386*plot.H_DOMINANTE.Value
            + 0.04917 * plot.SI.Value - 0.1991 * STR);
            	newTree.DAP+=(DBHG5-1);
        // calculo de la altura en el estado 2
        ///    	newTree.ALTURA+=HTG5/100;
	}
        /// <summary>
        /// Procedimiento que permite añadir nuevos árboles a una parcela después de "years" años
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <returns>Area basimetrica a distribuir o 0 si no hay masa incorporada</returns>
        public override double? AddTree(double years, Parcela plot)
        {
        double result = 1 / (1 + Math.Exp(-4.4765 + 0.1167 * plot.D_CUADRATICO.Value + 0.3492*plot.H_MEDIA.Value));
        if (result >= 0.58F)
            {
		    double N_Added = Math.Exp( 8.60716 - 0.64247*Math.Exp(plot.N_PIESHA.Value) + 197.99212/plot.D_MEDIO.Value);
		    if (N_Added<0)
		        {
            	return 0.0F;
		        }
            double BA_Added = Math.Pow(0.1/2,2)*Math.PI*N_Added;
            return BA_Added;
            }
            return 0.0F;
        }
        /// <summary>
        /// Expresa como se ha de distribuir la masa incorporada entre los árboles existentes.
        /// La implementación por defecto la distribuye de forma uniforme.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="AreaBasimetricaIncorporada"></param>
        /// <returns></returns>
        public override Distribution[] NewTreeDistribution(double years, Parcela plot, double AreaBasimetricaIncorporada)
        {
            Distribution[] distribution = new Distribution[3];
            double percentAreaBasimetrica = AreaBasimetricaIncorporada / plot.A_BASIMETRICA.Value;
            distribution[0] = new Distribution();
            distribution[0].diametroMenor = 7.5;
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
                tree.ALTURA = 1.3 + (3.099 - 0.00203*plot.A_BASIMETRICA.Value + 1.02491*plot.H_DOMINANTE.Value * Math.Exp(-8.5052/tree.DAP.Value));
                //double DIB = Math.Pow((1/1.3285),exp)*Math.Pow(tree.DAP.Value*10,exp);
                tree.VCC=0.000051*Math.Pow(tree.DAP.Value,1.86781)*Math.Pow(tree.ALTURA.Value,0.989625);
				/// almacenamiento de variables de biomasa (eq. para Q. pyrenaica, Ruiz-Peinado et al 2011)
				if (tree.ESPECIE==43)
				{
				/// Stem + Thick branches: Ws + Wb7 = 0.0261 · d2 · h
				tree.VAR_5 = 0.0261 * Math.Pow(tree.DAP.Value,2) * tree.ALTURA.Value;
				/// Medium branches: Wb2–7 = –0.0260 · d2 + 0.536 · h + 0.00538 · d2 · h 
				tree.VAR_2 = -0.0260 * Math.Pow(tree.DAP.Value,2) + 0.536 * tree.ALTURA.Value + 0.00538 * Math.Pow(tree.DAP.Value,2) * tree.ALTURA.Value;
				/// Thin branches: Wb2 = 0.898 · d – 0.445 · h  
				tree.VAR_3 = 0.898 * tree.DAP.Value - 0.445 * tree.ALTURA.Value;
				/// Roots: Wr = 0.143 · d2 
				tree.VAR_4 = 0.143 * Math.Pow(tree.DAP.Value,2);
				}
        }

        /// <summary>
        /// Procedimiento que realiza los cálculos sobre una parcela.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="trees"></param>
        public override void ProcessPlot(double years, Parcela plot, PieMayor[] trees)
        {
			double w_s_b7=0; double w_b27=0; double w_b2=0; double w_r=0 ;
			foreach (PieMayor tree in plot.PiesMayores)
            {
				if(tree.ESPECIE==43)
				{
				w_s_b7+=tree.VAR_5.Value*tree.EXPAN.Value;
				w_b27+=tree.VAR_2.Value*tree.EXPAN.Value;
				w_b2+=tree.VAR_3.Value*tree.EXPAN.Value;
				w_r+=tree.VAR_4.Value*tree.EXPAN.Value;	
				}
			}
			plot.VAR_2  = w_b27/1000;
			plot.VAR_3  = w_b2/1000;
			plot.VAR_4  = w_r/1000;
			plot.VAR_5  = w_s_b7/1000;
		}
    }
}


