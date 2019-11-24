using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
namespace EngineTest
{
    /// <summary>
    /// Todas las funciones y procedimientos son opcionales. Si se elimina cualquiera de ellas, se usará un
    /// procedimiento o funcion por defecto que no modifica el estado del inventario.
    /// Modelo IBERO, 2010, Pinus pinaster (Sistema Iberico)
    /// </summary>
    public class Template : ModelBase
    {
        /// declaracion de variables publicas
        public PieMayor currentTree;

        /// Funciones de perfil utilizadas en el cálculo de volumenes
        public double r2_conCorteza(double HR)
        {
	 	double r=(1+1.1034*Math.Exp(-6.0879*HR))*0.5656*currentTree.DAP.Value/200*Math.Pow((1-HR),(0.6330-1.7228*(1-HR)));
            	return Math.Pow(r,2);
        }
        public double r2_sinCorteza(double HR)
        {
	    	double r=(1+2.4771*Math.Exp(-5.0779*HR))*0.2360*currentTree.DAP.Value/200*Math.Pow(1-HR,0.4733-3.0371*(1-HR));
            	return Math.Pow(r,2);
        }
        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
	/// Solo se ejecuta en el primer nodo. 
	/// Variables que deben permanecer constantes como el índice de sitio deben calcularse solo en este apartado del modelo
        /// </summary>
        /// <param name="plot"></param>
        public override void CalculateInitialInventory(Parcela plot)
        {
			IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
			double bal = 0; double old_sec_normal = 100000; double old_bal=0; //double sec_normal = tree.SEC_NORMAL;
            foreach (PieMayor tree in piesOrdenados)
            {
		if (old_sec_normal>tree.SEC_NORMAL)	{tree.BAL=bal;old_bal=bal;}
			else {tree.BAL=old_bal;}
			bal	+=tree.SEC_NORMAL.Value*tree.EXPAN.Value/10000;
			old_sec_normal = tree.SEC_NORMAL.Value; 
            	if (!tree.ALTURA.HasValue||tree.ALTURA.Value==0)
            	{
                	tree.ALTURA=(13+(32.3287+1.6688*plot.H_DOMINANTE*10-0.1279*plot.D_CUADRATICO*10)*Math.Exp(-11.4522/Math.Sqrt(tree.DAP.Value*10)))/10.0;
            	}
            	if (!tree.ALTURA_MAC.HasValue||tree.ALTURA_MAC.Value==0)
            	{
                	tree.ALTURA_MAC=tree.ALTURA.Value/(1+Math.Exp((double)(-0.0041*tree.ALTURA.Value*10-0.0093*tree.BAL-0.0123*plot.A_BASIMETRICA)));
            	}
            	if (!tree.ALTURA_BC.HasValue||tree.ALTURA_BC.Value==0)
            	{
                	tree.ALTURA_BC=tree.ALTURA_MAC.Value/(1+Math.Exp((double)(0.0078*plot.A_BASIMETRICA-0.5488*Math.Log(plot.A_BASIMETRICA.Value)-0.0085*tree.BAL)));
            	}
            	tree.CR=1-tree.ALTURA_BC.Value/tree.ALTURA.Value;
            	if (!tree.LCW.HasValue)
            	{
                	tree.LCW=(1/10.0F)*(0.1826*tree.DAP.Value*10)*Math.Pow(tree.CR.Value,(0.1594+0.0014*(tree.ALTURA.Value-tree.ALTURA_BC.Value)*10));
            	}
		//		tree.VAR_1	= tree.COORD_X;//añadido para tener coordenadas
		//		tree.VAR_2	= tree.COORD_Y;//añadido para tener coordenadas
                currentTree = tree;
                tree.VCC=Math.PI*tree.ALTURA.Value*IntegralBySimpson(0,1,0.01,r2_conCorteza); //IntegraciÃ³n --> r2_conCorteza sobre HR en los limites 0 -> 1
            	tree.VSC=Math.PI*tree.ALTURA.Value*IntegralBySimpson(0,1,0.01,r2_sinCorteza); //IntegraciÃ³n --> r2_sinCorteza sobre HR en los limites 0 -> 1
                currentTree=null;
            }
            plot.SI = Math.Exp( 4.016 + 
			                   (Math.Log(plot.H_DOMINANTE.Value) - 4.016) * 
			                   Math.Pow(80/plot.EDAD.Value,-0.5031)
			                   );
        }
        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
        /// </summary>
        /// <param name="plot"></param>
        public override void Initialize(Parcela plot)
        {
        }
        /// <summary>
        /// Procedimiento que permite la inicialización de variables de árbol necesarias para la ejecución del modelo
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="tree"></param>
        public override void InitializeTree(Parcela plot, PieMayor tree)
        {
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
		double BA_Survives =1-(1/(1+Math.Exp(2.0968+(4.7358*tree.DAP.Value/plot.D_CUADRATICO.Value)-0.0012*plot.SI.Value*plot.A_BASIMETRICA.Value)));
		if (BA_Survives>0)
		{
           	return BA_Survives;
		}
		return 0.0F;
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
            	double DBHG5=Math.Exp(0.2030*Math.Log(oldTree.DAP.Value*10) + 0.4414*Math.Log((oldTree.CR.Value+0.2)/1.2) + 0.8379*Math.Log(plot.SI.Value) - 0.1295*Math.Sqrt(plot.A_BASIMETRICA.Value) - 0.0007*Math.Pow(oldTree.BAL.Value,2)/Math.Log(oldTree.DAP.Value*10));
            	newTree.DAP+=DBHG5/10;
		///    HTG5=(1/100)*Exp(0.21603+0.40329*Log(DBHG5*10/2)-1.12721*Log(DBH*10)                   +1.18099*Log(HT*100)                       +3.01622*CR)
		double HTG5=Math.Exp(0.21603+0.40329*Math.Log(DBHG5/2)-1.12721*Math.Log(oldTree.DAP.Value*10)+1.18099*Math.Log(oldTree.ALTURA.Value*100)+3.01622*oldTree.CR.Value);
            	newTree.ALTURA+=HTG5/100;
        	/// formula de la tesis--corregido error en base de datos    newTree.ALTURA+=(Math.Exp(4.1375+0.3762*Math.Log(DBHG5*10/2)-0.5260*Math.Log(oldTree.DAP.Value*10)+0.1727*Math.Log(oldTree.ALTURA.Value*100)+2.6468* oldTree.CR.Value))/100;
	}
        /// <summary>
        /// Procedimiento que permite añadir nuevos árboles a una parcela después de "years" años
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <returns>Area basimetrica a distribuir o 0 si no hay masa incorporada</returns>
        public override double? AddTree(double years, Parcela plot)
        {
            double result=1/(1+Math.Exp(12.3424+0.1108*plot.A_BASIMETRICA.Value-0.6154*plot.D_CUADRATICO.Value));
            if (result>=0.38F)
            {
		double BA_Added=6.7389-0.2235*plot.D_CUADRATICO.Value;
		if (BA_Added<0)
		{
            	return 0.0F;
		}
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
            distribution[0].diametroMenor = 0.0;
            distribution[0].diametroMayor = 12.5;
            distribution[0].AreaBasimetricaToAdd = 0.0809 * AreaBasimetricaIncorporada;
            distribution[1] = new Distribution();
            distribution[1].diametroMenor = 12.5;
            distribution[1].diametroMayor = 22.5;
            distribution[1].AreaBasimetricaToAdd = 0.3263 * AreaBasimetricaIncorporada;
            distribution[2] = new Distribution();
            distribution[2].diametroMenor = 22.5;
            distribution[2].diametroMayor = double.MaxValue;
            distribution[2].AreaBasimetricaToAdd = 0.5828 * AreaBasimetricaIncorporada;
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
            currentTree = tree;
            tree.VCC=Math.PI*tree.ALTURA.Value*IntegralBySimpson(0,1,0.01,r2_conCorteza); //Integración --> r2_conCorteza sobre HR en los limites 0 -> 1
            tree.VSC=Math.PI*tree.ALTURA.Value*IntegralBySimpson(0,1,0.01,r2_sinCorteza); //Integración --> d_sinCorteza sobre HR en los limites 0 -> 1
            currentTree = null;
        }

        /// <summary>
        /// Procedimiento que realiza los cálculos sobre una parcela.
        /// </summary>
        /// <param name="years"></param>
        /// <param name="plot"></param>
        /// <param name="trees"></param>
        public override void ProcessPlot(double years, Parcela plot, PieMayor[] trees)
        {
            IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
			double bal = 0; double old_sec_normal = 100000; double old_bal=0;
            foreach(PieMayor tree in piesOrdenados)
            {
				if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
				{
					if (old_sec_normal>tree.SEC_NORMAL)	{tree.BAL=bal;old_bal=bal;}
					else {tree.BAL=old_bal;}
					bal	+=tree.SEC_NORMAL.Value*tree.EXPAN.Value/10000;
					old_sec_normal = tree.SEC_NORMAL.Value; 

            		tree.ALTURA=(13+(32.3287+1.6688*plot.H_DOMINANTE*10-0.1279*plot.D_CUADRATICO*10)*Math.Exp(-11.4522/Math.Sqrt(tree.DAP.Value*10)))/10;
            		tree.ALTURA_MAC=tree.ALTURA.Value/(1+Math.Exp((double)(-0.0041*tree.ALTURA.Value*10-0.0093*tree.BAL-0.0123*plot.A_BASIMETRICA)));
            		tree.ALTURA_BC=tree.ALTURA_MAC.Value/(1+Math.Exp((double)(0.0078*plot.A_BASIMETRICA-0.5488*Math.Log(plot.A_BASIMETRICA.Value)-0.0085*tree.BAL)));
            		tree.CR=1-tree.ALTURA_BC.Value/tree.ALTURA.Value;
            		tree.LCW=(1/10.0F)*(0.1826*tree.DAP.Value*10)*Math.Pow(tree.CR.Value,(0.1594+0.0014*(tree.ALTURA.Value-tree.ALTURA_BC.Value)*10));
				}
				else{tree.BAL=0;	tree.CR=0;	tree.LCW = 0;	tree.ALTURA_MAC = 0;	tree.ALTURA_BC = 0;}
            }
        }
    }
}
