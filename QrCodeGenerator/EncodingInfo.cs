using System.Collections.Generic;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Details about the QR code encoding.
    /// <para>
    /// The details can be collected during QR code generation for analysis purposes,
    /// but they are not used by the library itself. Collecting the information will make
    /// the QR code generation slower as the library will fully calcuate the penalty score for
    /// all data mask patterns, even if it is already clear that the pattern is not the best one.
    /// </para>
    /// </summary>
    public class EncodingInfo
    {
        /// <summary>
        /// The penalty score for all eight possible data mask patterns.
        /// </summary>
        public PenaltyScore[] Penalties { get; } = new PenaltyScore[8];
        /// <summary>
        /// The data segments used to represent the payload data.
        /// </summary>
        public List<DataSegment> DataSegments { get; set; }
        /// <summary>
        /// The data mask to be forcibly applied to the data source, overriding the result of the automatic mask selection.
        /// <para>
        /// Set this property to a value between 0 and 7 to specify a particular data mask.
        /// A value of -1 indicates that the best data mask will be automatically selected.
        /// </para>
        /// </summary>
        public int ForcedDataMask { get; set; } = -1;
    }
}
