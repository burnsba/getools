# Gaudio tabledesign

Evaluates audio samples to create predictor codebook. Codebook predictors are required to compress a .wav file to .aifc.

Usage:

```
bin/tabledesign --in file
```

Options:

```
    --help                        print this help
    -n,--in=FILE                  input audio file to evaluate.
                                  Supported file formats:
                                  .wav .aifc
    -o,--out=FILE                 output file. Optional. If not provided, will
                                  reuse the input file name but change extension.
    --order=INT                   Number of lag samples. Default=2, min=1, max=8
    -p,--predictors=INT           Number of predictors. Default=1, min=1, max=8
    --threshold-mode=CHAR         Optional. Audio frame threshold filtering mode.
                                  If not supplied then no filtering is performed.
                                  Supported modes: a q.
    --threshold-min=DOUBLE        Threshold filter min value. Requries mode be set.
    --threshold-max=DOUBLE        Threshold filter max value. Requries mode be set.
    -q,--quiet                    suppress output
    -v,--verbose                  more output

threshold-mode = a

    'a' = "absolute" value filtering.
    Inner product of audio frame volume with itself is compared against threshold.
    Only frames with values above min and below max are considered.
    Frame value is compared directly against min and max.
    At least one of min or max must be supplied.

threshold-mode = q

    'q' = "quantile" value filtering.
    Inner product of audio frame volume with itself is compared against threshold.
    All frames are first measured to gather quantile statistics.
    The "threshold-min" parameter is interpreted as a quantile, between 0.0 and 1.0.
    The "threshold-max" parameter is interpreted the same way.
    Frame value is compared against the value at "min" and "max" quantile,
    only frames within this value range are accepted.
    At least one of min or max must be supplied.
```

# Parameters

There's probably no reason to change the "order" parameter from the default of 2.

The "predictors" parameter sets the number of predictors available to compress a file. Each "frame" (16 samples of 16-bit audio) chooses the best predictor for compression. Having more predictors may result in more accurate audio. Overall, the default value of 2 predictors is probably good enough (that's what the retail game uses).

The threshold parameter can be used to fine-tune which audio frames are used to build codebook coefficients. If you run `tabledesign` with the debug flag `--debug`, it will output some statistics for the frames it processed.

Example debug output:
```
ar_frame_count: 25404
frame measure mean, median, variance :
   8.68194e+08,    4.85731e+08,    1.21735e+18
frame measure min, max :
            48,    1.27323e+10
frame measure quantile 0.2, 0.4, 0.6, 0.8 :
   1.27787e+08,    3.41129e+08,    6.75855e+08,    1.36254e+09
```

This shows a mean threshold value of ~8.68e8 and max of ~1.27e10. From a small number of experiments, it seems filtering audio frames within a threshold range does not increase accuracy. Your mileage may vary.

See `src/lib/magic.c` for further details.

See the "Comparison of gaudio and N64 SDK tools" readme for a quantitative comparison to the N64 SDK `tabledesign` tool. Both have similar amount of error loss due to compression, but in slightly different ways.

# References

The following references were exceptionally helpful:

*Autoregressive Modelling for Speech Coding: Estimation, Interpolation and Quantisation*  
J.S. Erkelens

*Analysis and performance comparison of adaptive differential pulse code modulation data compression systems*  
1996  
Michael Vonshay Cooperwood  
https://calhoun.nps.edu/handle/10945/32143  

*Code-excited linear prediction (CELP)*  
Tom Bäckström  
Aug 04, 2020  
https://wiki.aalto.fi/pages/viewpage.action?pageId=149889236  

*Coding of Speech at 16 kbit/s Using Low-Delay Code Excited Linear Prediction*  
Recommendation G.728  
https://www.itu.int/rec/T-REC-G.728-199209-S/en  
