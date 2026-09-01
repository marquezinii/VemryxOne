import type { Metadata } from "next";
import "./globals.css";

const assetPrefix = process.env.NEXT_PUBLIC_BASE_PATH ?? "";
const socialImagePath = `${assetPrefix || "/Ralven"}/og.png`;

export const metadata: Metadata = {
  metadataBase: new URL("https://vemryx.com/"),
  title: "Ralven — Mais desempenho. Menos complicação.",
  description:
    "Diagnóstico, manutenção e otimização transparente do Windows, com recursos especializados para FiveM em GTA V Legacy.",
  applicationName: "Ralven",
  keywords: [
    "Ralven",
    "FiveM",
    "otimização Windows",
    "GTA V Legacy",
    "rollback",
    "Windows 11",
  ],
  authors: [{ name: "Ralven" }],
  creator: "Ralven",
  openGraph: {
    type: "website",
    locale: "pt_BR",
    alternateLocale: "en_US",
    title: "Ralven — Mais desempenho. Menos complicação.",
    description:
      "Otimização transparente do Windows, com diagnóstico, progresso claro, rollback e recursos especializados para FiveM.",
    siteName: "Ralven",
    images: [{ url: socialImagePath, width: 1672, height: 941 }],
  },
  twitter: {
    card: "summary_large_image",
    title: "Ralven",
    description: "Otimização transparente do Windows, com recursos especializados para FiveM.",
    images: [socialImagePath],
  },
  icons: {
    icon: [{ url: `${assetPrefix}/icon.png`, type: "image/png", sizes: "512x512" }],
    shortcut: `${assetPrefix}/icon.png`,
    apple: `${assetPrefix}/icon.png`,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR">
      <body>{children}</body>
    </html>
  );
}
