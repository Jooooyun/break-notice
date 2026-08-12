$ErrorActionPreference = 'Stop'

$showcaseRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$evidenceRoot = Join-Path $showcaseRoot 'Licenses\ProductionV002\Evidence'
New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null

$pages = @(
    @{ Name = 'LIC_LASONOTHEQUE_1226_PageDechireeFroissee2_20260812.html'; Url = 'https://lasonotheque.org/page-dechiree-et-froissee-2-s1226.html' },
    @{ Name = 'LIC_LASONOTHEQUE_1316_DiableManutention1_20260812.html'; Url = 'https://lasonotheque.org/diable-de-manutention-1-s1316.html' },
    @{ Name = 'LIC_LASONOTHEQUE_1318_PasRapidesBeton_20260812.html'; Url = 'https://lasonotheque.org/pas-rapides-sur-beton-s1318.html' },
    @{ Name = 'LIC_LASONOTHEQUE_1773_ChuteBarreAcier3_20260812.html'; Url = 'https://lasonotheque.org/chute-barre-d-acier-en-tole-3-s1773.html' },
    @{ Name = 'LIC_LASONOTHEQUE_1798_Whoosh10_20260812.html'; Url = 'https://lasonotheque.org/whoosh-10-s1798.html' },
    @{ Name = 'LIC_LASONOTHEQUE_1802_Whoosh9_20260812.html'; Url = 'https://lasonotheque.org/whoosh-9-s1802.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2244_BilleMetalRebond3_20260812.html'; Url = 'https://lasonotheque.org/bille-en-metal-rebond-3-s2244.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2359_BarriereMetalFermeture2_20260812.html'; Url = 'https://lasonotheque.org/grincement-barriere-metallique-fermeture-2-s2359.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2452_ChuteCorps1_20260812.html'; Url = 'https://lasonotheque.org/chute-de-corps-1-s2452.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2453_ChuteCorps2_20260812.html'; Url = 'https://lasonotheque.org/chute-de-corps-2-s2453.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2454_ChuteCorps3_20260812.html'; Url = 'https://lasonotheque.org/chute-de-corps-3-s2454.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2455_ChuteCorps4_20260812.html'; Url = 'https://lasonotheque.org/chute-de-corps-4-s2455.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2459_Punch4_20260812.html'; Url = 'https://lasonotheque.org/coup-de-poing-4-s2459.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2460_Punch5_20260812.html'; Url = 'https://lasonotheque.org/coup-de-poing-5-s2460.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2461_Punch6_20260812.html'; Url = 'https://lasonotheque.org/coup-de-poing-6-s2461.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2462_Punch7_20260812.html'; Url = 'https://lasonotheque.org/coup-de-poing-7-s2462.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2463_Punch8_20260812.html'; Url = 'https://lasonotheque.org/coup-de-poing-8-s2463.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2464_Punch9_20260812.html'; Url = 'https://lasonotheque.org/coup-de-poing-9-s2464.html' },
    @{ Name = 'LIC_LASONOTHEQUE_3296_HalterePose2_20260812.html'; Url = 'https://lasonotheque.org/haltere-pose-2-s3296.html' },
    @{ Name = 'LIC_LASONOTHEQUE_3441_PasGrincent2_20260812.html'; Url = 'https://lasonotheque.org/pas-qui-grincent-2-s3441.html' },
    @{ Name = 'LIC_LASONOTHEQUE_0516_ChaiseTombeMoquette_20260812.html'; Url = 'https://lasonotheque.org/chaise-en-bois-qui-tombe-sur-moquette-s0516.html' },
    @{ Name = 'LIC_LASONOTHEQUE_0666_ChaiseDeplacement_20260812.html'; Url = 'https://lasonotheque.org/chaise-s0666.html' },
    @{ Name = 'LIC_LASONOTHEQUE_0786_Classeur_20260812.html'; Url = 'https://lasonotheque.org/classeur-s0786.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2803_CarteRoutiere8_20260812.html'; Url = 'https://lasonotheque.org/carte-routiere-8-s2803.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2808_CarteRoutiere13_20260812.html'; Url = 'https://lasonotheque.org/carte-routiere-13-s2808.html' },
    @{ Name = 'LIC_LASONOTHEQUE_2902_CouverclePlastique8_20260812.html'; Url = 'https://lasonotheque.org/couvercle-plastique-8-s2902.html' },
    @{ Name = 'LIC_LASONOTHEQUE_Rights_20260812.html'; Url = 'https://lasonotheque.org/licences.html' },
    @{ Name = 'LIC_CC0_1.0_LegalCode_20260812.html'; Url = 'https://creativecommons.org/publicdomain/zero/1.0/legalcode' }
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$headers = @{ 'User-Agent' = 'BREAK-NOTICE-License-Evidence/1.0' }

foreach ($page in $pages) {
    $destination = Join-Path $evidenceRoot $page.Name
    Invoke-WebRequest -Uri $page.Url -UseBasicParsing -Headers $headers -OutFile $destination -MaximumRedirection 5
}

Write-Output "evidence_downloaded=$($pages.Count)"
