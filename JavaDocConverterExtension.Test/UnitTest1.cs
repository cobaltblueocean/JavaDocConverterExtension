using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JavaDocConverterExtension.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            String[] lines = new string[] { "import org.testng.annotations.Test;",
                                            "import org.threeten.bp.Period;",
                                            "import org.threeten.bp.ZonedDateTime;",
                                            "",
                                            "import com.opengamma.analytics.financial.instrument.annuity.*;",
                                            "import com.opengamma.analytics.financial.instrument.index.IndexON;",
                                            "import com.opengamma.test.analytics.financial.instrument.index.IndexONMaster;",
                                            "import com.opengamma.analytics.financial.instrument.payment.CouponONArithmeticAverageSpreadDefinition;",
                                            "import com.opengamma.analytics.financial.schedule.ScheduleCalculator;",
                                            "import com.opengamma.financial.convention.StubType;",
                                            "import com.opengamma.financial.convention.businessday.BusinessDayConvention;",
                                            "import com.opengamma.financial.convention.businessday.BusinessDayConventions;",
                                            "import com.opengamma.financial.convention.calendar.Calendar;",
                                            "import com.opengamma.financial.convention.calendar.MondayToFridayCalendar;",
                                            "import com.opengamma.util.test.TestGroup;",
                                            "import com.opengamma.util.time.DateUtils;" };

            String[] expect = new string[] { "Using org.testng.annotations;",
                                            "Using org.threeten.bp;",
                                            "",
                                            "",
                                            "Using com.opengamma.analytics.financial.instrument.annuity;",
                                            "Using com.opengamma.analytics.financial.instrument.index;",
                                            "Using com.opengamma.test.analytics.financial.instrument.index;",
                                            "Using com.opengamma.analytics.financial.instrument.payment;",
                                            "Using com.opengamma.analytics.financial.schedule;",
                                            "Using com.opengamma.financial.convention;",
                                            "Using com.opengamma.financial.convention.businessday;",
                                            "",
                                            "Using com.opengamma.financial.convention.calendar;",
                                            "",
                                            "Using com.opengamma.util.test;",
                                            "Using com.opengamma.util.time;" };

            for (int i = 0; i < lines.Length; i++)
            {
                if (!String.IsNullOrEmpty(lines[i]))
                {
                    if (lines[i].StartsWith("import"))
                    {
                        var buf = lines[i].Split(' ')[1].Split('.');
                        buf[buf.Length - 1] = "";
                        var tmp = "Using " + String.Join(".", buf) + ";";
                        lines[i] = tmp.Replace(".;", ";");
                    }
                }
            }

            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines.Length; j++)
                {
                    if ((i != j) && (lines[i] == lines[j]))
                        lines[j] = "";
                }
            }

            for (int i = 0; i < lines.Length; i++)
            {
                Assert.AreEqual(expect[i], lines[i]);
            }
        }
    }
}
