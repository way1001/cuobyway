#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

namespace ClassicUO.IO.Audio.MP3Sharp.Decoding.Decoders.LayerII
{
    /// <summary>
    ///     Class for layer II subbands in stereo mode.
    /// </summary>
    internal class SubbandLayer2Stereo : SubbandLayer2
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SubbandLayer2Stereo(int subbandnumber) : base(subbandnumber)
        {
            channel2_samples = new float[3];
        }

        protected internal int channel2_allocation;
        protected internal float[] channel2_c = { 0 };
        //protected boolean channel2_grouping;  ???? Never used!
        protected internal int[] channel2_codelength = { 0 };
        protected internal float[] channel2_d = { 0 };
        //protected float[][] channel2_groupingtable = {{0},{0}};
        protected internal float[] channel2_factor = { 0 };
        protected internal float[] channel2_samples;
        protected internal float channel2_scalefactor1, channel2_scalefactor2, channel2_scalefactor3;
        protected internal int channel2_scfsi;

        /// <summary>
        ///     *
        /// </summary>
        public override void read_allocation(Bitstream stream, Header header, Crc16 crc)
        {
            int length = get_allocationlength(header);
            allocation = stream.GetBitsFromBuffer(length);
            channel2_allocation = stream.GetBitsFromBuffer(length);

            if (crc != null)
            {
                crc.add_bits(allocation, length);
                crc.add_bits(channel2_allocation, length);
            }
        }

        /// <summary>
        ///     *
        /// </summary>
        public override void read_scalefactor_selection(Bitstream stream, Crc16 crc)
        {
            if (allocation != 0)
            {
                scfsi = stream.GetBitsFromBuffer(2);

                if (crc != null)
                {
                    crc.add_bits(scfsi, 2);
                }
            }

            if (channel2_allocation != 0)
            {
                channel2_scfsi = stream.GetBitsFromBuffer(2);

                if (crc != null)
                {
                    crc.add_bits(channel2_scfsi, 2);
                }
            }
        }

        /// <summary>
        ///     *
        /// </summary>
        public override void read_scalefactor(Bitstream stream, Header header)
        {
            base.read_scalefactor(stream, header);

            if (channel2_allocation != 0)
            {
                switch (channel2_scfsi)
                {
                    case 0:
                        channel2_scalefactor1 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        channel2_scalefactor2 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        channel2_scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];

                        break;

                    case 1:
                        channel2_scalefactor1 = channel2_scalefactor2 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        channel2_scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];

                        break;

                    case 2:

                        channel2_scalefactor1 = channel2_scalefactor2 = channel2_scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];

                        break;

                    case 3:
                        channel2_scalefactor1 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        channel2_scalefactor2 = channel2_scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];

                        break;
                }

                prepare_sample_reading
                (
                    header,
                    channel2_allocation,
                    1,
                    channel2_factor,
                    channel2_codelength,
                    channel2_c,
                    channel2_d
                );
            }
        }

        /// <summary>
        ///     *
        /// </summary>
        public override bool read_sampledata(Bitstream stream)
        {
            bool returnvalue = base.read_sampledata(stream);

            if (channel2_allocation != 0)
            {
                if (groupingtable[1] != null)
                {
                    int samplecode = stream.GetBitsFromBuffer(channel2_codelength[0]);
                    // create requantized samples:
                    samplecode += samplecode << 1;
                    /*
                    float[] target = channel2_samples;
                    float[] source = channel2_groupingtable[0];
                    int tmp = 0;
                    int temp = 0;
                    target[tmp++] = source[samplecode + temp];
                    temp++;
                    target[tmp++] = source[samplecode + temp];
                    temp++;
                    target[tmp] = source[samplecode + temp];
                    // memcpy (channel2_samples, channel2_groupingtable + samplecode, 3 * sizeof (real));
                    */
                    float[] target = channel2_samples;
                    float[] source = groupingtable[1];
                    int tmp = 0;
                    int temp = samplecode;
                    target[tmp] = source[temp];
                    temp++;
                    tmp++;
                    target[tmp] = source[temp];
                    temp++;
                    tmp++;
                    target[tmp] = source[temp];
                }
                else
                {
                    channel2_samples[0] = (float) (stream.GetBitsFromBuffer(channel2_codelength[0]) * channel2_factor[0] - 1.0);

                    channel2_samples[1] = (float) (stream.GetBitsFromBuffer(channel2_codelength[0]) * channel2_factor[0] - 1.0);

                    channel2_samples[2] = (float) (stream.GetBitsFromBuffer(channel2_codelength[0]) * channel2_factor[0] - 1.0);
                }
            }

            return returnvalue;
        }

        /// <summary>
        ///     *
        /// </summary>
        public override bool put_next_sample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            bool returnvalue = base.put_next_sample(channels, filter1, filter2);

            if (channel2_allocation != 0 && channels != OutputChannels.LEFT_CHANNEL)
            {
                float sample = channel2_samples[samplenumber - 1];

                if (groupingtable[1] == null)
                {
                    sample = (sample + channel2_d[0]) * channel2_c[0];
                }

                if (groupnumber <= 4)
                {
                    sample *= channel2_scalefactor1;
                }
                else if (groupnumber <= 8)
                {
                    sample *= channel2_scalefactor2;
                }
                else
                {
                    sample *= channel2_scalefactor3;
                }

                if (channels == OutputChannels.BOTH_CHANNELS)
                {
                    filter2.input_sample(sample, subbandnumber);
                }
                else
                {
                    filter1.input_sample(sample, subbandnumber);
                }
            }

            return returnvalue;
        }
    }
}